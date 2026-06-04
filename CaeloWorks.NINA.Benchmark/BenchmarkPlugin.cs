#region "copyright"
/*
    Copyright © 2026 CaeloWorks

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/
#endregion "copyright"
using System.ComponentModel.Composition;
using CaeloWorks.NINA.Benchmark.Core;
using NINA.Core.Interfaces;
using NINA.Image.ImageAnalysis;
using NINA.Image.Interfaces;
using NINA.Plugin;
using NINA.Plugin.Interfaces;
using NINA.Profile.Interfaces;

namespace CaeloWorks.NINA.Benchmark {

    /// <summary>
    /// Plugin entry point. The Options page (DataTemplate key "Benchmark_Options") is bound to this
    /// instance, so it exposes the shared <see cref="BenchmarkEngine"/> for the UI to bind against.
    /// </summary>
    [Export(typeof(IPluginManifest))]
    public class BenchmarkPlugin : PluginBase {

        [ImportingConstructor]
        public BenchmarkPlugin(IProfileService profileService, IImageDataFactory imageDataFactory,
            IPluggableBehaviorSelector<IStarDetection> starDetectionSelector) {
            Engine = BenchmarkEngine.GetInstance(profileService, imageDataFactory, starDetectionSelector);
        }

        /// <summary>Shared engine driving both the Options page and the Imaging dockables.</summary>
        public BenchmarkEngine Engine { get; }
    }
}
