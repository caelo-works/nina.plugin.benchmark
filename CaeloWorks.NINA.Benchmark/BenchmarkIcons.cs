#region "copyright"
/*
    Copyright © 2026 CaeloWorks

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/
#endregion "copyright"
using System.Windows.Media;

namespace CaeloWorks.NINA.Benchmark {

    /// <summary>
    /// Shared vector icons. N.I.N.A. fills dockable <c>ImageGeometry</c> (Stretch=Uniform), so the CPU
    /// glyph is a solid silhouette — a filled chip body with a central die and pins — generated in the
    /// spirit of Tabler's "cpu-2" icon. Built once and frozen so it is safe to share across threads.
    /// </summary>
    internal static class BenchmarkIcons {
        public static readonly GeometryGroup Cpu = BuildCpu();

        private static GeometryGroup BuildCpu() {
            // 24x24 view box. "F0" = EvenOdd: outer body ring + hole + central die, plus pins.
            const string data =
                "F0 " +
                "M5,5 H19 V19 H5 Z " +          // chip body (outer)
                "M8,8 H16 V16 H8 Z " +          // hole (carves the body into a frame)
                "M10.5,10.5 H13.5 V13.5 H10.5 Z " + // central die
                "M8.5,3 H10 V5 H8.5 Z M14,3 H15.5 V5 H14 Z " +     // top pins
                "M8.5,19 H10 V21 H8.5 Z M14,19 H15.5 V21 H14 Z " + // bottom pins
                "M3,8.5 H5 V10 H3 Z M3,14 H5 V15.5 H3 Z " +        // left pins
                "M19,8.5 H21 V10 H19 Z M19,14 H21 V15.5 H19 Z";    // right pins

            var group = new GeometryGroup { FillRule = FillRule.EvenOdd };
            group.Children.Add(Geometry.Parse(data));
            group.Freeze();
            return group;
        }
    }
}
