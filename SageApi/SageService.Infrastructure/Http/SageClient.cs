using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SageService.Domain.Entities;
using SageService.Domain.Interfaces;

namespace SageService.Infrastructure.Http
{
    /// <summary>
    /// Concrete Sage API client.
    /// - Reads BaseUrl from configuration: "Sage:BaseUrl" 
    /// - Adds api key and Basic auth headers per request.
    /// - Uses HttpClient (can be injected or created).
    /// </summary>
    public sealed class SageClient : ISageClient
    {
        private readonly HttpClient _http;
        private readonly string _baseUrl;
        private readonly ILogger<SageClient> _logger;

        public SageClient(IConfiguration config, ILogger<SageClient> logger) : this(new HttpClient(), config, logger)
        {
        }

        public SageClient(HttpClient httpClient, IConfiguration config, ILogger<SageClient> logger)
        {
            _http = httpClient ?? new HttpClient();
            _http.Timeout = TimeSpan.FromSeconds(60);

            var baseUrl = config["Sage:BaseUrl"];
            if (string.IsNullOrEmpty(baseUrl))
                throw new InvalidOperationException("Missing configuration key: Sage:BaseUrl");

            
            if (!baseUrl.EndsWith("/")) baseUrl += "/";
            _baseUrl = baseUrl;

            _logger = logger;
        }

        public async Task<int> EnsureCompanyIdAsync(ProviderApiCredentials creds, IProviderCompanyUpdater updater)
        {
            if (creds == null) throw new ArgumentNullException("creds");

            if (creds.CompanyId > 0)
                return creds.CompanyId;

            var path = "Company/Get";
            var json = await GetAsync(path, creds).ConfigureAwait(false);

            if (json != null && json["Results"] != null && json["Results"].HasValues)
            {
                var idToken = json["Results"][0]["ID"];
                if (idToken != null)
                {
                    var companyId = idToken.Value<int>();
                    creds.CompanyId = companyId;

                    if (updater != null && creds.ProviderId > 0)
                        await updater.UpdateCompanyIdAsync(creds.ProviderId, companyId).ConfigureAwait(false);

                    return companyId;
                }
            }
            return 0;
        }

        public async Task<JObject> GetAsync(string relativePath, ProviderApiCredentials creds)
        {
            var req = BuildRequest(HttpMethod.Get, relativePath, null, creds);
            using (var resp = await _http.SendAsync(req).ConfigureAwait(false))
            {
                var content = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                EnsureSuccess(resp, content);
                return Parse(content);
            }
        }

        public async Task<JObject> PostAsync(string relativePath, object body, ProviderApiCredentials creds)
        {
            var req = BuildRequest(HttpMethod.Post, relativePath, body, creds);
            using (var resp = await _http.SendAsync(req).ConfigureAwait(false))
            {
                var content = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                EnsureSuccess(resp, content);
                return Parse(content);
            }
        }

        public async Task<JObject> PutAsync(string relativePath, object body, ProviderApiCredentials creds)
        {
            var req = BuildRequest(HttpMethod.Put, relativePath, body, creds);
            using (var resp = await _http.SendAsync(req).ConfigureAwait(false))
            {
                var content = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                EnsureSuccess(resp, content);
                return Parse(content);
            }
        }

        public async Task<JObject> DeleteAsync(string relativePath, ProviderApiCredentials creds)
        {
            var req = BuildRequest(HttpMethod.Delete, relativePath, null, creds);
            using (var resp = await _http.SendAsync(req).ConfigureAwait(false))
            {
                var content = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                EnsureSuccess(resp, content);
                return Parse(content);
            }
        }

        public async Task<JObject> GetTaxTypesAsync(ProviderApiCredentials creds)
        {
            var companyId = await EnsureCompanyIdAsync(creds, null).ConfigureAwait(false);
            var path = string.Format("TaxType/Get?CompanyID={0}", companyId);
            return await GetAsync(path, creds).ConfigureAwait(false);
        }

        public async Task<JObject> GetItemsAsync(ProviderApiCredentials creds, string filter)
        {
            var companyId = await EnsureCompanyIdAsync(creds, null).ConfigureAwait(false);
            var path = string.Format("Item/Get?CompanyID={0}", companyId);
            if (!string.IsNullOrWhiteSpace(filter))
            {
                path = string.Format("{0}&filter={1}", path, Uri.EscapeDataString(filter));
            }
            return await GetAsync(path, creds).ConfigureAwait(false);
        }

        public async Task<JObject> SaveItemAsync(ProviderApiCredentials creds, object itemPayload)
        {
            var companyId = await EnsureCompanyIdAsync(creds, null).ConfigureAwait(false);
            var path = string.Format("Item/Save?CompanyID={0}", companyId);
            return await PostAsync(path, itemPayload, creds).ConfigureAwait(false);
        }

        public async Task<JObject> SaveCustomerAsync(ProviderApiCredentials creds, object customerPayload)
        {
            var companyId = await EnsureCompanyIdAsync(creds, null).ConfigureAwait(false);
            var path = string.Format("Customer/Save?CompanyID={0}", companyId);
            return await PostAsync(path, customerPayload, creds).ConfigureAwait(false);
        }

        public async Task<JObject> GetTaxInvoicesAsync(ProviderApiCredentials creds, bool includeDetail)
        {
            var companyId = await EnsureCompanyIdAsync(creds, null).ConfigureAwait(false);
            var path = string.Format("TaxInvoice/Get?includeDetail={0}&CompanyID={1}", includeDetail ? "true" : "false", companyId);
            return await GetAsync(path, creds).ConfigureAwait(false);
        }

        public async Task<JObject> SaveTaxInvoiceAsync(ProviderApiCredentials creds, object invoicePayload, bool useSystemDocumentNumber)
        {
            var companyId = await EnsureCompanyIdAsync(creds, null).ConfigureAwait(false);
            var path = string.Format("TaxInvoice/Save?useSystemDocumentNumber={0}&CompanyID={1}", useSystemDocumentNumber ? "true" : "false", companyId);
            return await PostAsync(path, invoicePayload, creds).ConfigureAwait(false);
        }

        public async Task<JObject> PostCustomerStatementsAsync(ProviderApiCredentials creds, int page, int pageSize, object body)
        {
            var companyId = await EnsureCompanyIdAsync(creds, null).ConfigureAwait(false);
            var path = string.Format("CustomerStatement/Get?page={0}&pageSize={1}&CompanyID={2}", page, pageSize, companyId);
            return await PostAsync(path, body, creds).ConfigureAwait(false);
        }
        private HttpRequestMessage BuildRequest(HttpMethod method, string relativePath, object body, ProviderApiCredentials creds)
        {
            if (creds == null) throw new ArgumentNullException("creds");

            var uri = new Uri(new Uri(_baseUrl), relativePath);

            var req = new HttpRequestMessage(method, uri);

            if (!string.IsNullOrEmpty(creds.ApiKey))
                req.Headers.Add("apikey", creds.ApiKey);

            var basic = BuildBasic(creds.Username, creds.Password);
            req.Headers.Authorization = new AuthenticationHeaderValue("Basic", basic);

            if (body != null && (method == HttpMethod.Post || method == HttpMethod.Put))
            {
                var json = JsonConvert.SerializeObject(body);
                req.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            return req;
        }

        private static string BuildBasic(string user, string pass)
        {
            var plain = string.Format("{0}:{1}", user ?? string.Empty, pass ?? string.Empty);
            var bytes = Encoding.ASCII.GetBytes(plain);
            return Convert.ToBase64String(bytes);
        }

        private static void EnsureSuccess(HttpResponseMessage resp, string payload)
        {
            if (!resp.IsSuccessStatusCode)
            {
                var msg = string.Format("Sage API request failed: {0} - {1}", resp.StatusCode, payload);
                throw new HttpRequestException(msg);
            }
        }

        private static JObject Parse(string content)
        {
            if (string.IsNullOrWhiteSpace(content)) return new JObject();
            return JObject.Parse(content);
        }
    }
}
