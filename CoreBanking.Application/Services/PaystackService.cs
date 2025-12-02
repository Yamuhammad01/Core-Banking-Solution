using CoreBanking.Application.Interfaces.IServices;
using CoreBanking.Application.Shared;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static CoreBanking.Application.Services.PaystackService;

namespace CoreBanking.Application.Services
{
    public interface IPayStackService 
    {
        Task<string> CreateVirtualAccountAsync(string customerCode);
        Task<string> CreateCustomerAsync(string firstName, string lastName, string email, string phone);

    }

    public class PaystackService : IPayStackService 
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        // private readonly string _secretKey;
        //private readonly string _secretKey = "sk_test_146f3cec11245e09635413a81bbcfbe47968cffe"; // Test Secret Key
        private readonly string _secretKey;

        public PaystackService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _config = configuration;
            _secretKey = _config["Paystack:SecretKey"];
            

            _httpClient.DefaultRequestHeaders.Authorization =
          new AuthenticationHeaderValue("Bearer", _secretKey);

            Console.WriteLine($"Auth Header Value: Bearer {_secretKey}");

            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }

     
        public async Task<string> CreateCustomerAsync(string firstName, string lastName, string email, string phone)
        {
            var customerData = new
            {
                first_name = firstName,
                last_name = lastName,
                email = email,
                phone = phone
            };

            var json = JsonConvert.SerializeObject(customerData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("customer", content);
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            dynamic responseObj = JsonConvert.DeserializeObject(responseString);

            // Return the customer code (e.g., "CUS_xxxx")
            return responseObj.data.customer_code;
        }

        public async Task<string> CreateVirtualAccountAsync(string customerCode)
        {
            var accountData = new
            {
                customer = customerCode,
                preferred_bank = "test-bank" // Used "test-bank" for sandbox testing
            };

            var json = JsonConvert.SerializeObject(accountData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("dedicated_account", content);
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            dynamic responseObj = JsonConvert.DeserializeObject(responseString);

            // Return the generated account number
            return responseObj.data.account_number;
        }

        public class PaystackVirtualAccountApiResponse
        {
            public bool Status { get; set; }
            public string Message { get; set; }
            public PaystackVirtualAccountData Data { get; set; }
        }

        public class PaystackVirtualAccountData
        {
            public string AccountNumber { get; set; }
            public string AccountName { get; set; }
            public string BankName { get; set; }
            public string CustomerCode { get; set; }
        }
        public class PaystackAccountRequest
        {
            public string CustomerId { get; set; }
            public string CustomerName { get; set; } = string.Empty;
            public string CustomerEmail { get; set; } = string.Empty;
        }
        public class PayStackCustomer 
        {
            public string Email { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Phone { get; set; }


        }

    }
}
