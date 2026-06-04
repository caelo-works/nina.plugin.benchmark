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
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace CaeloWorks.NINA.Benchmark.Core {

    /// <summary>
    /// Posts a benchmark run to the sharing site. The C# property names (PascalCase) are sent as-is
    /// and match the server's expected payload schema. Returns the shareable URL.
    /// </summary>
    public class BenchmarkSubmitter {
        private static readonly HttpClient Http = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
        private static readonly JsonSerializerOptions SerializeOptions = new() {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };
        private static readonly JsonSerializerOptions ReadOptions = new() { PropertyNameCaseInsensitive = true };

        public async Task<string> SubmitAsync(BenchmarkResult result, BenchmarkSettings settings,
            string pluginVersion, CancellationToken ct) {

            if (settings == null || string.IsNullOrWhiteSpace(settings.EndpointUrl)) {
                throw new InvalidOperationException(
                    "Set the submission endpoint URL in the plugin settings (Options page) first.");
            }

            var payload = new {
                schema = 1,
                pluginVersion,
                nickname = NullIfEmpty(settings.Nickname),
                machineName = NullIfEmpty(settings.MachineName),
                result,
            };

            var json = JsonSerializer.Serialize(payload, SerializeOptions);
            using var req = new HttpRequestMessage(HttpMethod.Post, settings.EndpointUrl.Trim()) {
                Content = new StringContent(json, Encoding.UTF8, "application/json"),
            };
            if (!string.IsNullOrWhiteSpace(settings.SubmitToken)) {
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.SubmitToken.Trim());
            }

            using var resp = await Http.SendAsync(req, ct);
            var body = await resp.Content.ReadAsStringAsync();
            if (!resp.IsSuccessStatusCode) {
                throw new Exception($"Server returned {(int)resp.StatusCode}. {Truncate(body, 200)}");
            }

            var parsed = JsonSerializer.Deserialize<SubmitResponse>(body, ReadOptions);
            return parsed?.Url ?? string.Empty;
        }

        private static string NullIfEmpty(string s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

        private static string Truncate(string s, int max) =>
            string.IsNullOrEmpty(s) ? "" : (s.Length <= max ? s : s.Substring(0, max) + "…");

        private class SubmitResponse {
            public string Id { get; set; }
            public string Url { get; set; }
        }
    }
}
