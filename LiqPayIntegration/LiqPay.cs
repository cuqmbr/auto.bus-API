using System.Buffers.Text;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using static System.Net.WebRequestMethods;

namespace LiqPayIntegration
{
    public class LiqPay
    {
        private readonly HttpClient _http;

        public const string API_URL = "https://www.liqpay.ua/api/request";
        public string PublicKey { get; init; }
        public string PrivateKey { get; init; }
        public LiqPay(string publicKey, string privateKey)
        {
            _http = new HttpClient();
            PublicKey = publicKey;
            PrivateKey = privateKey;
        }

        public async ValueTask<PaymentResponse[]> PaymentArchive(DateTime from, DateTime to)
        {
            var json = JsonContent.Create(new
            {
                action = "reports",
                version = 3,
                public_key = PublicKey,
                date_from = (long)from.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds,
                date_to = (long)to.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds
            });
            var data = Convert.ToBase64String(await json.ReadAsByteArrayAsync());
            var signString = PrivateKey + data + PrivateKey;
            var signature = Convert.ToBase64String(SHA1.HashData(Encoding.UTF8.GetBytes(signString)));
            var requestContent = new FormUrlEncodedContent(new[] {
                new KeyValuePair<string, string>("data", data),
                new KeyValuePair<string, string>("signature", signature)
            });
            var response = await _http.PostAsync(API_URL, requestContent);
            var rawResponse = await response.Content.ReadAsStringAsync();
            var jsonResponse = JsonNode.Parse(rawResponse);
            if (jsonResponse["result"] == null)
                throw new NullReferenceException("jsonResponse[\"result\"]");
            var result = jsonResponse["result"].Deserialize<string>();
            if (result != "success")
                throw new Exception("result isn't success");
            var paymentResponses = jsonResponse["data"].Deserialize<PaymentResponse[]>();
            return paymentResponses;
        }
    }
}