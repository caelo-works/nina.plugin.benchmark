#region "copyright"
/*
    Copyright © 2026 CaeloWorks

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/
#endregion "copyright"
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace CaeloWorks.NINA.Benchmark.Core {

    /// <summary>
    /// Posts a benchmark run to the public sharing site. The endpoint is a compile-time constant
    /// (no per-user configuration): change it here and recompile when the production site is deployed.
    /// Submissions are anonymous and public (no token). The C# property names (PascalCase) match the
    /// server's payload schema. Returns the shareable URL.
    /// </summary>
    public class BenchmarkSubmitter {
        // [DEPLOY] Test site for now; replace with the production URL and recompile.
        public const string Endpoint = "http://localhost:3000/api/runs";

        private static readonly HttpClient Http = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
        private static readonly JsonSerializerOptions SerializeOptions = new() {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };
        private static readonly JsonSerializerOptions ReadOptions = new() { PropertyNameCaseInsensitive = true };

        public async Task<string> SubmitAsync(BenchmarkResult result, string nickname, string machineName,
            string pluginVersion, CancellationToken ct) {

            var payload = new {
                schema = 1,
                pluginVersion,
                nickname = Clean(nickname, 40),
                machineName = Clean(machineName, 60),
                result,
            };

            var json = JsonSerializer.Serialize(payload, SerializeOptions);
            using var req = new HttpRequestMessage(HttpMethod.Post, Endpoint) {
                Content = new StringContent(json, Encoding.UTF8, "application/json"),
            };

            using var resp = await Http.SendAsync(req, ct);
            var body = await resp.Content.ReadAsStringAsync();
            if (!resp.IsSuccessStatusCode) {
                throw new Exception($"Server returned {(int)resp.StatusCode}. {Truncate(body, 200)}");
            }

            var parsed = JsonSerializer.Deserialize<SubmitResponse>(body, ReadOptions);
            return parsed?.Url ?? string.Empty;
        }

        private static string Clean(string s, int max) {
            if (string.IsNullOrWhiteSpace(s)) { return null; }
            s = s.Trim();
            return s.Length <= max ? s : s.Substring(0, max);
        }

        private static string Truncate(string s, int max) =>
            string.IsNullOrEmpty(s) ? "" : (s.Length <= max ? s : s.Substring(0, max) + "…");

        private class SubmitResponse {
            public string Id { get; set; }
            public string Url { get; set; }
        }
    }
}
