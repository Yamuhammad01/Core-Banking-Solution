using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreBanking.Application.Common
{
    public class Result
    {
        public bool Succeeded { get; set; }
        public string Message { get; set; }

        public static Result Success(string message) => new() { Succeeded = true, Message = message };
        public static Result Failure(string message) => new() { Succeeded = false, Message = message };
    }
}
