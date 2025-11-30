using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CoreBanking.Application.Shared
{
    public class MonnifyVirtualAccountApiResponse
    {
        [JsonPropertyName("requestSuccessful")]
        public bool RequestSuccessful { get; set; }

        [JsonPropertyName("responseMessage")]
        public string ResponseMessage { get; set; }

        [JsonPropertyName("responseCode")]
        public string ResponseCode { get; set; }

        [JsonPropertyName("responseBody")]
        public MonnifyVirtualAccountResponseBody ResponseBody { get; set; }
    }
    public class MonnifyVirtualAccountResponseBody
    {
        [JsonPropertyName("accountNumber")]
        public string AccountNumber { get; set; }

        [JsonPropertyName("bankName")]
        public string BankName { get; set; }

    }

}
