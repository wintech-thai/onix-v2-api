using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Its.Onix.Api.Models;

namespace Its.Onix.Api.Services
{
    // Scheme "PleasePayment" : เรียก public API ของ Please Payment เอง (SubmitPayInRequest) เพื่อขอ QR
    public class PleasePaymentGateway : IPaymentGateway
    {
        private readonly MPaymentGateway _config;

        public PleasePaymentGateway(MPaymentGateway config)
        {
            _config = config;
        }

        public async Task<PaymentGatewayQrResult> GenerateQR(PaymentGatewayQrRequest request)
        {
            var result = new PaymentGatewayQrResult();

            if (string.IsNullOrEmpty(_config.EndpointBaseUrl) || string.IsNullOrEmpty(_config.EndpointOrgId) ||
                string.IsNullOrEmpty(_config.EndpointMerchantId) || string.IsNullOrEmpty(_config.ApiKey))
            {
                result.Status = "PAYMENT_GATEWAY_CONFIG_MISSING";
                result.Description = $"Payment gateway [{_config.Code}] is missing endpoint config (EndpointBaseUrl / EndpointOrgId / EndpointMerchantId / ApiKey)";
                return result;
            }

            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                    "Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"api:{_config.ApiKey}")));

                var body = new Dictionary<string, object?>
                {
                    ["RefId1"] = request.Ref1,
                    ["RefId2"] = request.Ref2,
                    ["PayerName"] = request.Payer,
                    ["RequestedAmount"] = request.Amount,
                    ["Currency"] = "THB",
                    ["QrProvider"] = "PP",
                    ["Description"] = "Point top-up",
                    ["CustomerEmail"] = request.Payer,
                };

                var httpReq = new HttpRequestMessage(
                    HttpMethod.Post,
                    $"{_config.EndpointBaseUrl!.TrimEnd('/')}/api/PaymentRequest/org/{_config.EndpointOrgId}/action/SubmitPayInRequest/{_config.EndpointMerchantId}")
                {
                    Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json"),
                };

                var response = await client.SendAsync(httpReq);
                var responseBody = await response.Content.ReadAsStringAsync();

                using var doc = JsonDocument.Parse(responseBody);
                var root = doc.RootElement;

                var status = root.TryGetProperty("status", out var statusEl) ? statusEl.GetString() : null;
                if (!response.IsSuccessStatusCode || status != "OK" || !root.TryGetProperty("paymentResponse", out var pr))
                {
                    var desc = root.TryGetProperty("description", out var descEl) ? descEl.GetString() : responseBody;
                    result.Status = "PAYMENT_GATEWAY_API_ERROR";
                    result.Description = $"Please Payment SubmitPayInRequest failed [{(int)response.StatusCode}] : {desc}";
                    return result;
                }

                result.Status = "OK";
                result.Description = "Success";
                result.QrCode = pr.TryGetProperty("qrCode", out var qrEl) ? qrEl.GetString() : null;
                result.QrCodeImageBase64 = pr.TryGetProperty("qrCodeImage", out var imgEl) ? imgEl.GetString() : null;
                result.PaymentUrl = pr.TryGetProperty("paymentUrl", out var urlEl) ? urlEl.GetString() : null;
                result.ExternalPaymentRequestId = pr.TryGetProperty("id", out var idEl) ? idEl.GetString() : null;
            }
            catch (Exception ex)
            {
                result.Status = "PAYMENT_GATEWAY_API_ERROR";
                result.Description = $"Failed to call Please Payment SubmitPayInRequest : {ex.Message}";
            }

            return result;
        }
    }
}
