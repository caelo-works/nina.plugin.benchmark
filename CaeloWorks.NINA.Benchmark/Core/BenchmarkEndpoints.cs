#region "copyright"
/*
    Copyright © 2026 CaeloWorks

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/
#endregion "copyright"
namespace CaeloWorks.NINA.Benchmark.Core {

    /// <summary>
    /// Where the plugin talks to the sharing site. Compile-time constants (no per-user
    /// configuration): change <see cref="BaseUrl"/> and recompile when the production site
    /// is deployed.
    /// </summary>
    public static class BenchmarkEndpoints {
        // [DEPLOY] Homelab CT (LAN). Swap for the public domain once the reverse proxy is up.
        public const string BaseUrl = "http://10.0.1.189:3000";

        /// <summary>Submission endpoint (POST a run).</summary>
        public const string Runs = BaseUrl + "/api/runs";

        /// <summary>Single-use nonce for signing a submission (GET).</summary>
        public const string Challenge = BaseUrl + "/api/challenge";

        /// <summary>Test-set manifest (GET name/size/sha256/url for every benchmark frame).</summary>
        public const string TestSet = BaseUrl + "/api/testset";
    }
}
