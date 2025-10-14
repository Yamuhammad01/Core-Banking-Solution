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
        public object Data { get; set; }

        public ApiResponses() { }

        public ApiResponses(bool success, string message = null, object data = null)
        {
            Success = success;
            Message = message;
            Data = data;
        }

        public static ApiResponses SuccessResponse(string message = "Request successful", object data = null)
        {
            return new ApiResponses(true, message, data);
        }

        public static ApiResponses FailResponse(string message = "Request failed")
        {
            return new ApiResponses(false, message, null);
        }
    }
}
