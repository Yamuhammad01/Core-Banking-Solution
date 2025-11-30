using CoreBanking.Application.Interfaces.IServices;
using CoreBanking.Application.Shared;
using CoreBanking.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CoreBanking.Application.Services
{
    public class MonnifyService : IMonnifyService
    {
        private readonly HttpClient _client;
        private readonly IConfiguration _config;
        private string _accessToken = null;
        private DateTime _tokenExpiry = DateTime.MinValue;
        public MonnifyService(HttpClient client, IConfiguration config)
        {
            _client = client;
            _config = config;
        }
        private async Task EnsureTokenAsync()
        {
            if (!string.IsNullOrEmpty(_accessToken) && DateTime.UtcNow < _tokenExpiry)
                return;

            var apiKey = _config["Monnify:ApiKey"];
            var secretKey = _config["Monnify:SecretKey"];
            var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{apiKey}:{secretKey}"));

            var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/login");
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64);

            var response = await _client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadFromJsonAsync<MonnifyTokenResponse>();
            if (json == null || !json.RequestSuccessful)
                throw new Exception("Monnify authentication failed");

            _accessToken = json.ResponseBody.AccessToken;
            _tokenExpiry = DateTime.UtcNow.AddSeconds(json.ResponseBody.ExpiresIn - 1000); // refresh 60s early

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
        }

        public async Task<MonnifyAccountResponse> CreateDedicatedVirtualAccountAsync(MonnifyAccountRequest request)
        {
            await EnsureTokenAsync(); // call the auth method

            var contractCode = _config["Monnify:ContractCode"];
            if (string.IsNullOrWhiteSpace(request.CustomerId))
                request.CustomerId = Guid.NewGuid().ToString();

            if (string.IsNullOrWhiteSpace(request.CustomerName))
                throw new Exception("CustomerName cannot be empty");

            if (string.IsNullOrWhiteSpace(request.CustomerEmail))
                throw new Exception("CustomerEmail cannot be empty");

            if (string.IsNullOrWhiteSpace(contractCode))
                throw new Exception("Monnify ContractCode missing");
            // 
            string bvn = "22222222222";
            var payload = new
            {
                accountReference = request.CustomerId,
                accountName = request.CustomerName,
                currencyCode = "NGN",
                contractCode = contractCode,
                customerEmail = request.CustomerEmail,
                bvn = bvn,
                customerName = request.CustomerName,   // correct casing!
                getAllAvailableBanks = true
            };

            var response = await _client.PostAsJsonAsync("/api/v1/bank-transfer/reserved-accounts", payload);

            var responseText = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("Monnify Error Response: " + responseText);
                return new MonnifyAccountResponse
                {
                    Success = false,
                    ErrorMessage = responseText
                };
            }
            Console.WriteLine(JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true }));


            var json = await response.Content.ReadFromJsonAsync<MonnifyVirtualAccountApiResponse>(
               new System.Text.Json.JsonSerializerOptions
               {
                  PropertyNameCaseInsensitive = true
              });


            if (json == null || json.RequestSuccessful == false)
                return new MonnifyAccountResponse { Success = false };

            return new MonnifyAccountResponse
            {
                Success = true,
                AccountNumber = json.ResponseBody.AccountNumber,
                BankName = json.ResponseBody.BankName
            };
        }
        public class MonnifyTokenResponse
        {
            public bool RequestSuccessful { get; set; }
            public string ResponseMessage { get; set; }
            public string ResponseCode { get; set; }
            public MonnifyTokenResponseBody ResponseBody { get; set; }
        }
        public class MonnifyTokenResponseBody
        {
            public string AccessToken { get; set; }
            public int ExpiresIn { get; set; } // seconds
        }
    }
}
