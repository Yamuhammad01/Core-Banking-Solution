using Octokit.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreBanking.Application.Responses
{
    public class ApiResponses
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string? Reference { get; set; }
        public decimal NewBalance { get; set; }

        public ApiResponses() { }

        public ApiResponses(bool success, string message = null, string reference = null, decimal newBalance = 0)
        {
            Success = success;
            Message = message;
            Reference = reference;
            NewBalance = newBalance;
        }

        public static ApiResponses SuccessResponse(string message = "Request successful")
        {
            return new ApiResponses(true, message);
        }

        public static ApiResponses FailResponse(string message = "Request failed")
        {
            return new ApiResponses(false, message);
        }
    }
}
