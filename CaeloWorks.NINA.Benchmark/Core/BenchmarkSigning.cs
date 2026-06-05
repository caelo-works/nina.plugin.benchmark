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
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace CaeloWorks.NINA.Benchmark.Core {

    /// <summary>
    /// Signs a submission so the server can tell it came from a genuine plugin build (and not a
    /// blind <c>curl</c>). This is a deliberate *speed bump*: the key below is embedded in the DLL
    /// and therefore extractable — it only stops casual tampering. The key is rotated per release
    /// (the server maps plugin version → key) so a leaked key dies with its version.
    ///
    /// CANONICAL FORMAT v1 — must match src/lib/sign.ts on the server EXACTLY.
    ///   IntMs(x)      = round(x*100) as an integer
    ///   FunctionsHash = sha256hex( functions
    ///                     .Select(f => $"{Name}:{IntMs(Ms)}:{IncludeInTotal?1:0}:{Applicable?1:0}")
    ///                     .Join(";") )
    ///   Canonical     = ["v1", schema, pluginVersion, testSetVersion, nonce,
    ///                    score, IntMs(totalMs), imageCount, runs, totalStars,
    ///                    FunctionsHash].Join("|")
    ///   Signature     = hmacSha256hex(key=hexBytes(Key), utf8(Canonical))
    /// </summary>
    public static class BenchmarkSigning {
        // [DEPLOY] Shared with the server's SUBMIT_KEYS entry for THIS plugin version.
        // Generate a fresh one (openssl rand -hex 32) whenever the version changes.
        private const string Key = "c379f5666b9db48d2276c45aff2ff21e0cc6833f30f95781bb5ec1ca5f5a93d2";

        public static string Sign(string canonical) {
            using var h = new HMACSHA256(Convert.FromHexString(Key));
            return ToHexLower(h.ComputeHash(Encoding.UTF8.GetBytes(canonical)));
        }

        public static string Canonical(int schema, string pluginVersion, string testSetVersion, string nonce,
            double score, double totalMs, int imageCount, int runs, int totalStars, IEnumerable<FunctionResult> functions) {
            var parts = new[] {
                "v1",
                Inv(schema),
                pluginVersion ?? "",
                testSetVersion ?? "",
                nonce ?? "",
                ScoreKey(score),
                IntMs(totalMs),
                Inv(imageCount),
                Inv(runs),
                Inv(totalStars),
                FunctionsHash(functions),
            };
            return string.Join("|", parts);
        }

        private static string FunctionsHash(IEnumerable<FunctionResult> functions) {
            var joined = string.Join(";", functions.Select(f =>
                f.Name + ":" + IntMs(f.Ms) + ":" + (f.IncludeInTotal ? "1" : "0") + ":" + (f.Applicable ? "1" : "0")));
            using var sha = SHA256.Create();
            return ToHexLower(sha.ComputeHash(Encoding.UTF8.GetBytes(joined)));
        }

        // round(x*100) as an integer, avoiding any float-formatting mismatch across .NET / JS.
        // AwayFromZero on .5 ties matches JavaScript's Math.round for the (non-negative) ms here.
        private static string IntMs(double x) =>
            ((long)Math.Round(x * 100.0, MidpointRounding.AwayFromZero)).ToString(CultureInfo.InvariantCulture);

        // Score carries one decimal, so bind it as integer tenths (same rounding rule as IntMs).
        private static string ScoreKey(double score) =>
            ((long)Math.Round(score * 10.0, MidpointRounding.AwayFromZero)).ToString(CultureInfo.InvariantCulture);

        private static string Inv(int v) => v.ToString(CultureInfo.InvariantCulture);

        private static string ToHexLower(byte[] bytes) {
            var c = new char[bytes.Length * 2];
            for (var i = 0; i < bytes.Length; i++) {
                var b = bytes[i];
                c[i * 2] = HexDigit(b >> 4);
                c[i * 2 + 1] = HexDigit(b & 0xF);
            }
            return new string(c);
        }

        private static char HexDigit(int v) => (char)(v < 10 ? '0' + v : 'a' + (v - 10));
    }
}
