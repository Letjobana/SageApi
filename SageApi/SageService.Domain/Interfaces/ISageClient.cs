using Newtonsoft.Json.Linq;
using SageService.Domain.Entities;

namespace SageService.Domain.Interfaces
{
    /// <summary>
    /// Minimal client abstraction over SageOne/Accounting API.
    /// Keeps auth + base URL handling in one place.
    /// Returns JSON (JObject/JArray).
    /// </summary>
    public interface ISageClient
    {
        /// <summary>
        /// Ensures CompanyId is set on the credential;
        /// </summary>
        /// <param name="creds">"Credentials</param>
        /// <param name="updater">Updater</param>
        /// <returns></returns>
        Task<int> EnsureCompanyIdAsync(ProviderApiCredentials creds, IProviderCompanyUpdater updater);

        /// <summary>
        /// GET raw endpoint path (relative to BaseUrl)
        /// </summary>
        /// <param name="relativePath">Relative Path</param>
        /// <param name="creds">Credentials</param>
        /// <returns></returns>
        Task<JObject> GetAsync(string relativePath, ProviderApiCredentials creds);

        /// <summary>
        /// POST JSON to endpoint; returns parsed JSON.
        /// </summary>
        /// <param name="relativePath">Relative Path</param>
        /// <param name="body">Body</param>
        /// <param name="creds">Credentials</param>
        /// <returns></returns>
        Task<JObject> PostAsync(string relativePath, object body, ProviderApiCredentials creds);

        /// <summary>
        /// PUT JSON to endpoint; returns parsed JSON.
        /// </summary>
        /// <param name="relativePath">Relative Path</param>
        /// <param name="body">Body</param>
        /// <param name="creds">Credentials</param>
        /// <returns></returns>
        Task<JObject> PutAsync(string relativePath, object body, ProviderApiCredentials creds);

        /// <summary>
        /// DELETE to endpoint; returns parsed JSON (if any)
        /// </summary>
        /// <param name="relativePath">Relative Path</param>
        /// <param name="creds">Credentials</param>
        /// <returns></returns>
        Task<JObject> DeleteAsync(string relativePath, ProviderApiCredentials creds);

        /// <summary>
        /// Gets tax types.
        /// </summary>
        /// <param name="creds">Credentials</param>
        /// <returns></returns>
        Task<JObject> GetTaxTypesAsync(ProviderApiCredentials creds);

        /// <summary>
        /// Gets items by filter (e.g., code eq 'X')
        /// </summary>
        /// <param name="creds">Credentials</param>
        /// <param name="filter">Filter</param>
        /// <returns></returns>
        Task<JObject> GetItemsAsync(ProviderApiCredentials creds, string filter);

        /// <summary>
        /// Saves/creates an item
        /// </summary>
        /// <param name="creds">Credentials</param>
        /// <param name="itemPayload">Item Pay load</param>
        /// <returns></returns>
        Task<JObject> SaveItemAsync(ProviderApiCredentials creds, object itemPayload);

        /// <summary>
        /// Saves/creates a customer
        /// </summary>
        /// <param name="creds">Credentials</param>
        /// <param name="customerPayload">Customer Pay load</param>
        /// <returns></returns>
        Task<JObject> SaveCustomerAsync(ProviderApiCredentials creds, object customerPayload);

        /// <summary>
        /// Gets tax invoices
        /// </summary>
        /// <param name="creds">Credentials</param>
        /// <param name="includeDetail">Include Detail</param>
        /// <returns></returns>
        Task<JObject> GetTaxInvoicesAsync(ProviderApiCredentials creds, bool includeDetail);

        /// <summary>
        /// Saves/creates a tax invoice
        /// </summary>
        /// <param name="creds">Credentials</param>
        /// <param name="invoicePayload">Invoice Payload</param>
        /// <param name="useSystemDocumentNumber">Use System Document Number</param>
        /// <returns></returns>
        Task<JObject> SaveTaxInvoiceAsync(ProviderApiCredentials creds, object invoicePayload, bool useSystemDocumentNumber);

        /// <summary>
        /// Posts CustomerStatement
        /// </summary>
        /// <param name="creds">Credentials</param>
        /// <param name="page">Invoice Payload</param>
        /// <param name="pageSize"></param>
        /// <param name="body">Body</param>
        /// <returns></returns>
        Task<JObject> PostCustomerStatementsAsync(ProviderApiCredentials creds, int page, int pageSize, object body);
    }
    /// <summary>
    /// Small abstraction so SageClient can persist newly discovered CompanyId without referencing repository directl
    /// </summary>
    public interface IProviderCompanyUpdater
    {
        Task UpdateCompanyIdAsync(int providerId, int companyId);
    }
}
