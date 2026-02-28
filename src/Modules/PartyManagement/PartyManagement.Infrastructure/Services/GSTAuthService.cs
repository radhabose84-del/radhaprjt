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
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public GSTAuthService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
        }

        public async Task<GSTAuthResponseDto> GetAuthTokenAsync()
        {
         /*    var payload = new
            {
                username = _config["GSTApi:UserName"],
                password = _config["GSTApi:Password"],
                AppKey = _config["GSTApi:AppKey"],
                ForceRefreshAccessToken = false
            }; */

            var encrypted = "F+d1cLTFFM3PY/dt/U4flmc718WopRlemfg/DXCPWefDU33Z5icEC+Rdbx7vBDUvX7Ht0dnuxy0ILSiOcqBSHB9y0mqmQ1T+eFx1k+0+BkZHb0B17A5s5Ct+DgXfzWWLEyeN+YVfUxHFD6CADiRvW9Qm9rgjkjxVft+zEe/aU0vT19cxhdmkoZvb/0RcG1Oj/pDUxvtjvBZ0wFcK3HCgfyM5DhwkzQ5v7jU2sVLNzwoP4+Qd6EIPhcAsrLCCJB8V6QS7T1dMMj6KHLdNVTuXdoQY7HR7HmFEun2AMptjA3zBdYv2W0evUgXbIXQZr3nvRBOQok2BFZaqC228KOn4AQ==";
            var body = new { Data = encrypted };

            var request = new HttpRequestMessage(HttpMethod.Post, $"{_config["GSTApi:BaseUrl"]}/auth")
            {
                Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json")
            };

            request.Headers.Add("client_id", _config["GSTApi:ClientId"]);
            request.Headers.Add("client_secret", _config["GSTApi:ClientSecret"]);
            request.Headers.Add("gstin", _config["GSTApi:Gstin"]);

            var response = await _httpClient.SendAsync(request);
            string content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"NIC Auth API failed: {content}");

            return JsonConvert.DeserializeObject<GSTAuthResponseDto>(content);
        }

        public async Task<GSTINDetailsDto> GetGSTINDetailsAsync(string gstin)
        {
            var auth = await GetAuthTokenAsync();
            string appKey = _config["GSTApi:AppKey"];

            var request = new HttpRequestMessage(HttpMethod.Get, $"{_config["GSTApi:BaseUrl"]}/Master/gstin/{gstin}");
            request.Headers.Add("client_id", auth.Data.ClientId);
            request.Headers.Add("client_secret", _config["GSTApi:ClientSecret"]);
            request.Headers.Add("gstin", _config["GSTApi:Gstin"]);
            request.Headers.Add("user_name", auth.Data.UserName);   // ✅ must be user_name
            request.Headers.Add("AuthToken", auth.Data.AuthToken);
            request.Headers.Add("Sek", auth.Data.Sek);

            var response = await _httpClient.SendAsync(request);
            string content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"NIC GSTIN API failed: {content}");

            // Deserialize first
            var rawResponse = JsonConvert.DeserializeObject<dynamic>(content);
            string encryptedData = rawResponse?.Data;

            // ✅ Step 1: Decrypt Sek
            string decryptedSek = GstDecryptionHelper.DecryptSek(auth.Data.Sek, appKey);

            // ✅ Step 2: Decrypt Data
            string decryptedJson = GstDecryptionHelper.DecryptData(encryptedData, decryptedSek);

            // ✅ Step 3: Map to DTO
            return JsonConvert.DeserializeObject<GSTINDetailsDto>(decryptedJson);
        }
    }
}
