#region "copyright"
/*
    Copyright © 2026 CaeloWorks

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/
#endregion "copyright"
using System.ComponentModel.Composition;
using System.Windows;

namespace CaeloWorks.NINA.Benchmark.View {

    /// <summary>
    /// Exported so N.I.N.A. merges these DataTemplates into the application resources, making the
    /// Options page ("Benchmark_Options") and the two dockable templates resolvable by key.
    /// </summary>
    [Export(typeof(ResourceDictionary))]
    public partial class PluginDataTemplates : ResourceDictionary {
        public PluginDataTemplates() {
            InitializeComponent();
        }
    }
}
