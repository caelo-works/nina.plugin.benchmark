#region "copyright"
/*
    Copyright © 2026 CaeloWorks

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/
#endregion "copyright"
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NINA.Core.Enum;
using NINA.Core.Interfaces;
using NINA.Image.ImageAnalysis;
using NINA.Image.Interfaces;
using NINA.Profile.Interfaces;

namespace CaeloWorks.NINA.Benchmark.Core {

    /// <summary>
    /// Shared, long-lived service that owns the benchmark state (system info, history, run command).
    /// Exposed as a process-wide singleton via <see cref="GetInstance"/> rather than a MEF export, so
    /// the plugin Options page and the two Imaging dockables bind the very same instance even though
    /// N.I.N.A. composes the plugin manifest and the dockables in separate passes.
    /// </summary>
    public partial class BenchmarkEngine : ObservableObject {
        private static readonly string[] FrameExtensions = { ".fits", ".fit", ".fts", ".xisf" };
        private static readonly string[] BayerHints = { "osc", "color", "colour", "bayer", "rggb" };

        private readonly IProfileService profileService;
        private readonly IImageDataFactory imageDataFactory;
        private readonly IPluggableBehaviorSelector<IStarDetection> starDetectionSelector;
        private readonly BenchmarkResultStore store;
        private readonly SystemInfoProvider systemInfoProvider;
        private readonly BenchmarkSettingsStore settingsStore;
        private readonly BenchmarkSubmitter submitter;
        private readonly string pluginVersion;

        private CancellationTokenSource cts;

        [ObservableProperty] private SystemInfo systemInfo;
        [ObservableProperty] private bool isRunning;
        [ObservableProperty] private double progressValue;
        [ObservableProperty] private string statusText = "Idle";
        [ObservableProperty] private int iterations = 3;

        // The only user-configurable submission field (persisted to settings.json on change).
        [ObservableProperty] private string machineName;
        [ObservableProperty] private bool isSubmitting;
        [ObservableProperty] private string lastShareUrl;

        /// <summary>Convenience for binding button enabled-state without a converter.</summary>
        public bool NotSubmitting => !IsSubmitting;

        /// <summary>Where runs are submitted (compile-time constant), shown read-only in the UI.</summary>
        public string SubmissionEndpoint => BenchmarkSubmitter.Endpoint;

        /// <summary>The NINA Observer name used to label submissions (read-only here).</summary>
        public string Observer => profileService?.ActiveProfile?.AstrometrySettings?.Observer ?? "";

        /// <summary>Active profile name, the default machine label when none is set.</summary>
        public string ActiveProfileName => profileService?.ActiveProfile?.Name ?? "";

        public ObservableCollection<BenchmarkResult> History { get; }

        /// <summary>Most recent run, used by the UI to show the per-function breakdown.</summary>
        public BenchmarkResult LatestResult => History.Count > 0 ? History[0] : null;

        /// <summary>Highest score across the kept history (0 when empty).</summary>
        public int BestScore => History.Count > 0 ? History.Max(h => h.Score) : 0;

        public string TestImagesFolder { get; }

        private static BenchmarkEngine instance;
        private static readonly object gate = new object();

        /// <summary>Returns the single shared engine, creating it from the injected services on first use.</summary>
        public static BenchmarkEngine GetInstance(IProfileService profileService, IImageDataFactory imageDataFactory,
            IPluggableBehaviorSelector<IStarDetection> starDetectionSelector) {
            if (instance == null) {
                lock (gate) {
                    if (instance == null) {
                        instance = new BenchmarkEngine(profileService, imageDataFactory, starDetectionSelector);
                    }
                }
            }
            return instance;
        }

        private BenchmarkEngine(IProfileService profileService, IImageDataFactory imageDataFactory,
            IPluggableBehaviorSelector<IStarDetection> starDetectionSelector) {
            this.profileService = profileService;
            this.imageDataFactory = imageDataFactory;
            this.starDetectionSelector = starDetectionSelector;

            store = new BenchmarkResultStore();
            systemInfoProvider = new SystemInfoProvider();
            settingsStore = new BenchmarkSettingsStore();
            submitter = new BenchmarkSubmitter();
            pluginVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "";
            TestImagesFolder = ResolveTestImagesFolder();
            History = new ObservableCollection<BenchmarkResult>(store.Load());

            // Assign backing field directly so loading doesn't trigger a save.
            machineName = settingsStore.Load().MachineName;

            _ = RefreshSystemInfoAsync();
        }

        partial void OnIsSubmittingChanged(bool value) => OnPropertyChanged(nameof(NotSubmitting));

        partial void OnMachineNameChanged(string value) =>
            settingsStore.Save(new BenchmarkSettings { MachineName = MachineName ?? "" });

        [RelayCommand]
        private async Task SubmitAsync(BenchmarkResult result) {
            if (result == null || IsSubmitting) { return; }
            IsSubmitting = true;
            StatusText = "Submitting…";
            try {
                // Identity comes from N.I.N.A.: the Observer option, and the machine label
                // (the configured name, or the active profile name as a fallback).
                var observer = profileService?.ActiveProfile?.AstrometrySettings?.Observer;
                var machine = string.IsNullOrWhiteSpace(MachineName)
                    ? profileService?.ActiveProfile?.Name
                    : MachineName;

                var url = await submitter.SubmitAsync(result, observer, machine, pluginVersion, CancellationToken.None);

                LastShareUrl = url;
                if (!string.IsNullOrEmpty(url)) {
                    result.ShareUrl = url;   // flips the row's Share button to "View"
                    store.Save(History);     // persist so it survives restarts
                }
                TryCopyToClipboard(url);
                StatusText = string.IsNullOrEmpty(url) ? "Submitted." : $"Shared — link copied: {url}";
            } catch (Exception ex) {
                StatusText = "Submit failed: " + ex.Message;
            } finally {
                IsSubmitting = false;
            }
        }

        [RelayCommand]
        private void OpenShareUrl() => OpenInBrowser(LastShareUrl);

        [RelayCommand]
        private void OpenRun(BenchmarkResult result) => OpenInBrowser(result?.ShareUrl);

        private static void OpenInBrowser(string url) {
            if (string.IsNullOrWhiteSpace(url)) { return; }
            try {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(url) {
                    UseShellExecute = true,
                });
            } catch { /* ignore */ }
        }

        private static void TryCopyToClipboard(string text) {
            if (string.IsNullOrEmpty(text)) { return; }
            try {
                System.Windows.Clipboard.SetText(text);
            } catch { /* clipboard not available */ }
        }

        [RelayCommand]
        private async Task RefreshSystemInfoAsync() {
            SystemInfo = await systemInfoProvider.GetAsync();
        }

        private bool CanRun() => !IsRunning;

        [RelayCommand(CanExecute = nameof(CanRun))]
        private async Task RunBenchmarkAsync() {
            cts = new CancellationTokenSource();
            IsRunning = true;
            RunBenchmarkCommand.NotifyCanExecuteChanged();
            ClearHistoryCommand.NotifyCanExecuteChanged();
            ProgressValue = 0;
            StatusText = "Preparing…";
            try {
                var frames = DiscoverFrames();
                // Refresh the system snapshot at the start of the run so it reflects the machine state
                // (power plan, free RAM, clocks) at benchmark time and is stored with the result.
                StatusText = "Reading system info…";
                SystemInfo = await systemInfoProvider.GetAsync();

                var progress = new Progress<BenchmarkProgress>(p => {
                    ProgressValue = p.Fraction;
                    StatusText = p.Status;
                });

                var runner = new BenchmarkRunner(profileService, imageDataFactory, starDetectionSelector.GetBehavior());
                var token = cts.Token;
                var result = await Task.Run(() => runner.RunAsync(frames, Iterations, progress, token), token);

                result.System = SystemInfo;
                History.Insert(0, result);
                while (History.Count > BenchmarkResultStore.MaxEntries) {
                    History.RemoveAt(History.Count - 1);
                }
                OnPropertyChanged(nameof(LatestResult));
                OnPropertyChanged(nameof(BestScore));
                store.Save(History);
                StatusText = $"Done — score {result.Score} ({result.TotalMs:0} ms total)";
            } catch (OperationCanceledException) {
                StatusText = "Cancelled";
            } catch (Exception ex) {
                StatusText = "Error: " + ex.Message;
            } finally {
                ProgressValue = 0;
                IsRunning = false;
                RunBenchmarkCommand.NotifyCanExecuteChanged();
                ClearHistoryCommand.NotifyCanExecuteChanged();
            }
        }

        [RelayCommand]
        private void Cancel() => cts?.Cancel();

        [RelayCommand]
        private void ToggleDetails(BenchmarkResult result) {
            if (result != null) {
                result.IsExpanded = !result.IsExpanded;
            }
        }

        [RelayCommand(CanExecute = nameof(CanRun))]
        private void ClearHistory() {
            History.Clear();
            store.Save(History);
            OnPropertyChanged(nameof(LatestResult));
            OnPropertyChanged(nameof(BestScore));
            StatusText = "History cleared";
        }

        private IReadOnlyList<TestFrame> DiscoverFrames() {
            if (string.IsNullOrEmpty(TestImagesFolder) || !Directory.Exists(TestImagesFolder)) {
                return Array.Empty<TestFrame>();
            }
            return Directory.EnumerateFiles(TestImagesFolder, "*.*", SearchOption.AllDirectories)
                .Where(f => FrameExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
                .OrderBy(f => f)
                .Select(f => {
                    var lower = f.ToLowerInvariant();
                    return new TestFrame {
                        Path = f,
                        Name = Path.GetFileName(f),
                        IsBayered = BayerHints.Any(h => lower.Contains(h)),
                        BayerPattern = SensorType.RGGB
                    };
                })
                .ToList();
        }

        private static string ResolveTestImagesFolder() {
            var asmDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            return asmDir == null ? null : Path.Combine(asmDir, "TestImages");
        }
    }
}
