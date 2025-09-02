using Microsoft.Extensions.Configuration;
using SageService.Domain.Entities;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;

namespace SageService.Infrastructure
{
    public static class Helpers
    {
        /// <summary>
        /// Helper utilities for calling the Sage One API:
        /// - Building HttpClient with headers
        /// - URL composition
        /// - JSON content helpers
        /// - Common data shaping (names, "N/A")
        /// - Tax type lookups
        /// </summary>
        public static class SageApiHelper
        {
            public static HttpClient BuildClient(IConfiguration config, string apiKey, string username, string password)
            {
                var baseUrl = config["SageApiBaseUrl"];
                if (string.IsNullOrWhiteSpace(baseUrl))
                    throw new InvalidOperationException("SageApiBaseUrl is not configured.");

                var client = new HttpClient
                {
                    Timeout = TimeSpan.FromSeconds(30)
                };

                client.DefaultRequestHeaders.Add("apikey", apiKey);

                var basic = Convert.ToBase64String(Encoding.ASCII.GetBytes(username + ":" + password));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", basic);

                return client;
            }

            /// <summary>
            /// Combines configured Sage API root with a relative path, trimming duplicated slashes.
            /// </summary>
            public static string Combine(IConfiguration config, string relative)
            {
                var root = config["SageApiBaseUrl"] ?? string.Empty;
                root = root.TrimEnd('/');
                relative = (relative ?? string.Empty).TrimStart('/');
                return root + "/" + relative;
            }

            /// <summary>
            /// Escapes single quotes for OData-like filter segments (e.g., Code eq 'X' ).
            /// </summary>
            public static string EscapeForFilter(string s)
            {
                return (s ?? string.Empty).Replace("'", "''");
            }

            /// <summary>
            /// Serializes an object into JSON content with UTF8 and application/json.
            /// </summary>
            public static StringContent ToJson(object obj)
            {
                var json = JsonSerializer.Serialize(obj);
                return new StringContent(json, Encoding.UTF8, "application/json");
            }

            /// <summary>
            /// Converts null/empty strings into "N/A" (Sage rejects empty address/phone values).
            /// </summary>
            public static string NullOrNA(string s)
            {
                return string.IsNullOrWhiteSpace(s) ? "N/A" : s;
            }

            /// <summary>
            /// Builds a full name from a learner. Falls back to DisplayName or "Unknown".
            /// </summary>
            public static string BuildFullName(Learner learner)
            {
                if (learner == null) return "Unknown";
                var name = learner.FullName?.Trim();
                return string.IsNullOrWhiteSpace(name) ? "Unknown" : name;
            }

            /// <summary>
            /// Looks up the Sage "Standard" VAT tax type and returns its ID.
            /// </summary>
            public static async Task<(int salesTaxTypeId, int purchasesTaxTypeId)> GetStandardTaxTypeIdsAsync(IConfiguration config, HttpClient client, string apiKey,int companyId,CancellationToken ct)
            {
                var url = Combine(config, "TaxType/Get?apikey=" + apiKey + "&CompanyID=" + companyId);

                using (var resp = await client.GetAsync(url, ct))
                {
                    var json = await resp.Content.ReadAsStringAsync(ct);
                    resp.EnsureSuccessStatusCode();

                    using var doc = JsonDocument.Parse(json);
                    if (!doc.RootElement.TryGetProperty("Results", out var results) || results.ValueKind != JsonValueKind.Array)
                        return (1, 1);

                    var standard = results.EnumerateArray().FirstOrDefault(x => x.TryGetProperty("Name", out var n) &&
                                  (n.GetString() ?? "").IndexOf("Standard", StringComparison.OrdinalIgnoreCase) >= 0);

                    if (standard.ValueKind == JsonValueKind.Object && standard.TryGetProperty("ID", out var idEl))
                    {
                        var id = idEl.GetInt32();
                        return (id, id);
                    }
                }
                return (1, 1);
            }
        }
    }
}
