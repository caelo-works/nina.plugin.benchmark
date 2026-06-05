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
using NINA.Equipment.Interfaces.ViewModel;
using NINA.Image.ImageAnalysis;
using NINA.Image.Interfaces;
using NINA.Profile.Interfaces;
using NINA.WPF.Base.ViewModel;

namespace CaeloWorks.NINA.Benchmark.Dockables {

    /// <summary>
    /// Imaging-view dockable showing the host system information. Bound to the shared
    /// <see cref="BenchmarkEngine"/> via the "..._Dockable" DataTemplate.
    /// </summary>
    [Export(typeof(IDockableVM))]
    public class BenchmarkSystemInfoDockable : DockableVM {

        [ImportingConstructor]
        public BenchmarkSystemInfoDockable(IProfileService profileService, IImageDataFactory imageDataFactory,
            IPluggableBehaviorSelector<IStarDetection> starDetectionSelector) : base(profileService) {
            Engine = BenchmarkEngine.GetInstance(profileService, imageDataFactory, starDetectionSelector);
            Title = "Benchmark System";
            CanClose = true;
            ImageGeometry = BenchmarkIcons.Cpu;
        }

        public BenchmarkEngine Engine { get; }
    }
}
