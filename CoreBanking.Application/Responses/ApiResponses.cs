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

        public ApiResponses() { }

        public ApiResponses(bool success, string message = null)
        {
            Success = success;
            Message = message;
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
