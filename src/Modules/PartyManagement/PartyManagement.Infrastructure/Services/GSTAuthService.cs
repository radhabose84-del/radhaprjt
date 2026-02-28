#nullable disable
using System.Text;
using System.Text.RegularExpressions;
using Contracts.Common;
using PartyManagement.Application.GST.DTOs;
using PartyManagement.Application.Interfaces.GST;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace PartyManagement.Infrastructure.Services
{
    public class GSTAuthService : IGSTAuthService
    {
        private static readonly Regex GstinRegex = new(
            "^[0-9]{2}[A-Z]{5}[0-9]{4}[A-Z][1-9A-Z]Z[0-9A-Z]$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public GSTAuthService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
        }

        public async Task<GSTAuthResponseDto> GetAuthTokenAsync()
        {
            var encrypted = "F+d1cLTFFM3PY/dt/U4flmc718WopRlemfg/DXCPWefDU33Z5icEC+Rdbx7vBDUvX7Ht0dnuxy0ILSiOcqBSHB9y0mqmQ1T+eFx1k+0+BkZHb0B17A5s5Ct+DgXfzWWLEyeN+YVfUxHFD6CADiRvW9Qm9rgjkjxVft+zEe/aU0vT19cxhdmkoZvb/0RcG1Oj/pDUxvtjvBZ0wFcK3HCgfyM5DhwkzQ5v7jU2sVLNzwoP4+Qd6EIPhcAsrLCCJB8V6QS7T1dMMj6KHLdNVTuXdoQY7HR7HmFEun2AMptjA3zBdYv2W0evUgXbIXQZr3nvRBOQok2BFZaqC228KOn4AQ==";
            var body = new { Data = encrypted };

            var baseUrl = GetRequiredConfig("GSTApi:BaseUrl");
            var clientId = GetRequiredConfig("GSTApi:ClientId");
            var clientSecret = GetRequiredConfig("GSTApi:ClientSecret");
            var companyGstin = GetRequiredConfig("GSTApi:Gstin");

            var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/auth")
            {
                Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json")
            };

            request.Headers.Add("client_id", clientId);
            request.Headers.Add("client_secret", clientSecret);
            request.Headers.Add("gstin", companyGstin);

            var response = await _httpClient.SendAsync(request);
            string content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new ExceptionRules("GST authentication failed. Please try again later.");

            var auth = JsonConvert.DeserializeObject<GSTAuthResponseDto>(content);
            if (auth?.Data == null ||
                string.IsNullOrWhiteSpace(auth.Data.ClientId) ||
                string.IsNullOrWhiteSpace(auth.Data.UserName) ||
                string.IsNullOrWhiteSpace(auth.Data.AuthToken) ||
                string.IsNullOrWhiteSpace(auth.Data.Sek))
            {
                throw new ExceptionRules("Unable to authenticate GST service. Please try again later.");
            }

            return auth;
        }

        public async Task<GSTINDetailsDto> GetGSTINDetailsAsync(string gstin)
        {
            if (string.IsNullOrWhiteSpace(gstin))
                throw new ExceptionRules("GSTIN is required.");

            gstin = gstin.Trim().ToUpperInvariant();
            if (!GstinRegex.IsMatch(gstin))
                throw new ExceptionRules("Invalid GSTIN format.");

            var auth = await GetAuthTokenAsync();
            string appKey = GetRequiredConfig("GSTApi:AppKey");
            var baseUrl = GetRequiredConfig("GSTApi:BaseUrl");
            var clientSecret = GetRequiredConfig("GSTApi:ClientSecret");
            var companyGstin = GetRequiredConfig("GSTApi:Gstin");

            var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}/Master/gstin/{gstin}");
            request.Headers.Add("client_id", auth.Data.ClientId);
            request.Headers.Add("client_secret", clientSecret);
            request.Headers.Add("gstin", companyGstin);
            request.Headers.Add("user_name", auth.Data.UserName);
            request.Headers.Add("AuthToken", auth.Data.AuthToken);
            request.Headers.Add("Sek", auth.Data.Sek);

            var response = await _httpClient.SendAsync(request);
            string content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                if ((int)response.StatusCode == 404)
                    throw new ExceptionRules("No records found for the given GSTIN.");

                if ((int)response.StatusCode == 400 || (int)response.StatusCode == 422)
                    throw new ExceptionRules("Invalid GSTIN. Please enter a valid GSTIN.");

                throw new ExceptionRules("Unable to fetch GST details from GST provider. Please try again later.");
            }

            var rawResponse = JsonConvert.DeserializeObject<dynamic>(content);
            string encryptedData = rawResponse?.Data;
            if (string.IsNullOrWhiteSpace(encryptedData))
                throw new ExceptionRules("No records found for the given GSTIN.");

            string decryptedSek;
            string decryptedJson;
            try
            {
                decryptedSek = GstDecryptionHelper.DecryptSek(auth.Data.Sek, appKey);
                decryptedJson = GstDecryptionHelper.DecryptData(encryptedData, decryptedSek);
            }
            catch
            {
                throw new ExceptionRules("Unable to process GST provider response. Please try again later.");
            }

            if (string.IsNullOrWhiteSpace(decryptedJson))
                throw new ExceptionRules("No records found for the given GSTIN.");

            var details = JsonConvert.DeserializeObject<GSTINDetailsDto>(decryptedJson);
            return details ?? throw new ExceptionRules("No records found for the given GSTIN.");
        }

        private string GetRequiredConfig(string key)
        {
            var value = _config[key];
            if (string.IsNullOrWhiteSpace(value))
                throw new ExceptionRules($"Missing configuration: {key}");

            return value;
        }
    }
}
