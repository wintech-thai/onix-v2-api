using System.Globalization;
using System.Text;
using System.Text.Json;
using Its.Onix.Api.Models;
using Its.Onix.Api.Utils;

namespace Its.Onix.Api.Services
{
    public class QrGeneratorSCB : IQrGenerator
    {
        private readonly MPaymentRequest _pqymentRequest;
        private readonly MBankAccount _bankAccount;
        private readonly IRedisHelper _redis;

        //อ้างอิงจาก https://developer.scb/#/documents/api-reference-index/qr-payments/post-qrcode-create.html
        private const string SandboxBaseUrl = "https://api-sandbox.partners.scb/partners/sandbox";
        //TODO : ยังไม่ confirm ค่าจริงของ Production base url จาก SCB (ต้องขอเอกสารจากธนาคารตอนขึ้น production จริง) ใส่ไว้ตามรูปแบบที่คาดว่าน่าจะใกล้เคียง
        private const string ProductionBaseUrl = "https://api.partners.scb/partners";

        public QrGeneratorSCB(MPaymentRequest pmr, MBankAccount ba, IRedisHelper redis)
        {
            _pqymentRequest = pmr;
            _bankAccount = ba;
            _redis = redis;
        }

        public async Task<QrGeneratorResult> GenerateAsync()
        {
            var result = new QrGeneratorResult()
            {
                Status = "OK",
                Description = "Success",
            };

            //GetPayInBankAccount() เรียก repository ตรง ๆ ไม่ผ่าน service layer ที่ทำ deserialize BankConfig -> BankConfigObj ไว้
            //เลย fallback deserialize เองตรงนี้ ถ้า BankConfigObj ยังไม่ถูกตั้งค่าแต่มี BankConfig (raw string) อยู่
            var cfg = _bankAccount.BankConfigObj;
            if (cfg == null && !string.IsNullOrEmpty(_bankAccount.BankConfig))
            {
                try { cfg = JsonSerializer.Deserialize<MBankAccountConfig>(_bankAccount.BankConfig); }
                catch { cfg = null; }
            }

            if (cfg == null || string.IsNullOrEmpty(cfg.ApiKey) || string.IsNullOrEmpty(cfg.ApiSecret) || string.IsNullOrEmpty(cfg.BillerId))
            {
                result.Status = "SCB_CONFIG_MISSING";
                result.Description = $"Bank account [{_bankAccount.Id}] is missing SCB API config (ApiKey / ApiSecret / BillerId) - กรอกที่หน้า Bank API Config ก่อน";
                return result;
            }

            var baseUrl = cfg.IsSandbox ? SandboxBaseUrl : ProductionBaseUrl;

            string accessToken;
            string tokenType;
            try
            {
                (accessToken, tokenType) = await GetAccessTokenAsync(baseUrl, cfg);
            }
            catch (Exception ex)
            {
                result.Status = "SCB_AUTH_FAILED";
                result.Description = $"Failed to get SCB access token: {ex.Message}";
                return result;
            }

            try
            {
                using var client = new HttpClient();

                //ref1 ใช้ matching ตอน webhook กลับมา
                var refValue = _pqymentRequest.RefId ?? Guid.NewGuid().ToString("N");
                //ref2 ต้องมีค่าเสมอ (Biller ตั้ง Two references ไว้)
                var ref2Value = !string.IsNullOrWhiteSpace(_pqymentRequest.RefId2) ? _pqymentRequest.RefId2 : refValue;
                //ref3 ต้องขึ้นต้นด้วย Ref3Prefix เสมอ
                var ref3Suffix = !string.IsNullOrWhiteSpace(_pqymentRequest.RefId3) ? _pqymentRequest.RefId3 : refValue;
                var ref3 = $"{cfg.Ref3Prefix}{ref3Suffix}";

                var body = new Dictionary<string, object?>
                {
                    ["qrType"] = "PP", //ค่า "PP" ของ SCB หมายถึง QR 30 (คนละความหมายกับ QrProvider="PP" ของเราที่หมายถึง PromptPay เอง)
                    ["amount"] = (_pqymentRequest.GeneratedAmount ?? 0).ToString("F2", CultureInfo.InvariantCulture),
                    ["ppType"] = "BILLERID",
                    ["ppId"] = cfg.BillerId,
                    ["ref1"] = refValue,
                    ["ref2"] = ref2Value,
                    ["ref3"] = ref3,
                };

                var bodyJson = JsonSerializer.Serialize(body);
                var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/v1/payment/qrcode/create")
                {
                    Content = new StringContent(bodyJson, Encoding.UTF8, "application/json"),
                };
                request.Headers.Add("resourceOwnerId", cfg.ApiKey);
                request.Headers.Add("requestUId", Guid.NewGuid().ToString("N"));
                request.Headers.Add("authorization", $"{tokenType} {accessToken}");
                request.Headers.Add("accept-language", "EN");

                //TODO : เอา log ตรงนี้ออกตอนใช้งานจริง เพราะ accessToken/billerId เป็นข้อมูล sensitive - ใส่ไว้ debug ตอนเทส sandbox เท่านั้น
                Console.WriteLine($"INFO : [QrGeneratorSCB] qrcode/create request body : {bodyJson}");

                var response = await client.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"INFO : [QrGeneratorSCB] qrcode/create response [{(int)response.StatusCode}] : {responseBody}");

                using var doc = JsonDocument.Parse(responseBody);
                var root = doc.RootElement;

                if (!response.IsSuccessStatusCode || !root.TryGetProperty("data", out var data) ||
                    !data.TryGetProperty("qrRawData", out var qrRawDataEl) || string.IsNullOrEmpty(qrRawDataEl.GetString()))
                {
                    var desc = root.TryGetProperty("status", out var statusEl) && statusEl.TryGetProperty("description", out var descEl)
                        ? descEl.GetString()
                        : responseBody;
                    result.Status = "SCB_API_ERROR";
                    result.Description = $"SCB qrcode/create failed [{(int)response.StatusCode}] : {desc}";
                    return result;
                }

                result.QrPayload = qrRawDataEl.GetString() ?? "";
                var qrImageBase64 = data.TryGetProperty("qrImage", out var qrImageEl) ? qrImageEl.GetString() : null;
                result.ImageBytes = string.IsNullOrEmpty(qrImageBase64) ? Array.Empty<byte>() : Convert.FromBase64String(qrImageBase64);
            }
            catch (Exception ex)
            {
                result.Status = "SCB_API_ERROR";
                result.Description = $"Failed to call SCB qrcode/create : {ex.Message}";
            }

            return result;
        }

        //ดึง JWT token จาก Redis ถ้ายังไม่ expire ไม่งั้นขอใหม่จาก SCB แล้วเก็บกลับเข้า cache
        private async Task<(string accessToken, string tokenType)> GetAccessTokenAsync(string baseUrl, MBankAccountConfig cfg)
        {
            var cacheKey = $"{CacheHelper.CreateBankApiTokenKey(_bankAccount.OrgId ?? "global", "SCBToken")}:{_bankAccount.Id}:{cfg.ApiKey}";

            var cached = await _redis.GetObjectAsync<ScbTokenCache>(cacheKey);
            var nowEpoch = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (cached != null && cached.ExpiresAt > nowEpoch + 10) //เผื่อ buffer 10 วินาทีก่อนหมดอายุจริง กัน race condition
            {
                return (cached.AccessToken, cached.TokenType);
            }

            using var client = new HttpClient();

            var body = new
            {
                applicationKey = cfg.ApiKey,
                applicationSecret = cfg.ApiSecret,
            };

            var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/v1/oauth/token")
            {
                Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json"),
            };
            request.Headers.Add("resourceOwnerId", cfg.ApiKey);
            request.Headers.Add("requestUId", Guid.NewGuid().ToString("N"));
            request.Headers.Add("accept-language", "EN");

            var response = await client.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();

            //TODO : เอา log ตรงนี้ออกตอนใช้งานจริง เพราะ responseBody มี accessToken อยู่ด้วย - ใส่ไว้ debug ตอนเทส sandbox เท่านั้น
            Console.WriteLine($"INFO : [QrGeneratorSCB] oauth/token response [{(int)response.StatusCode}] : {responseBody}");

            using var doc = JsonDocument.Parse(responseBody);
            var root = doc.RootElement;

            if (!response.IsSuccessStatusCode || !root.TryGetProperty("data", out var data) ||
                !data.TryGetProperty("accessToken", out var accessTokenEl) || string.IsNullOrEmpty(accessTokenEl.GetString()))
            {
                var desc = root.TryGetProperty("status", out var statusEl) && statusEl.TryGetProperty("description", out var descEl)
                    ? descEl.GetString()
                    : responseBody;
                throw new Exception($"SCB oauth/token failed [{(int)response.StatusCode}] : {desc}");
            }

            var accessToken = accessTokenEl.GetString()!;
            var tokenType = data.TryGetProperty("tokenType", out var tokenTypeEl) ? (tokenTypeEl.GetString() ?? "Bearer") : "Bearer";
            var expiresIn = data.TryGetProperty("expiresIn", out var expiresInEl) ? expiresInEl.GetInt64() : 1800; //default เผื่อ SCB ไม่ส่ง expiresIn มา

            var tokenCache = new ScbTokenCache
            {
                AccessToken = accessToken,
                TokenType = tokenType,
                ExpiresAt = nowEpoch + expiresIn,
            };

            var ttl = TimeSpan.FromSeconds(Math.Max(1, expiresIn - 10)); //หัก buffer 10 วิ ก่อนหมดอายุจริง
            _ = _redis.SetObjectAsync(cacheKey, tokenCache, ttl);

            return (accessToken, tokenType);
        }

        public QrGeneratorResult Generate()
        {
            throw new NotImplementedException();
        }

        //เช็คสถานะ payment เองแทนรอ webhook
        public async Task<ScbInquiryResult> InquireAsync(string transactionDate)
        {
            var result = new ScbInquiryResult()
            {
                Status = "OK",
                Description = "Success",
            };

            var cfg = _bankAccount.BankConfigObj;
            if (cfg == null && !string.IsNullOrEmpty(_bankAccount.BankConfig))
            {
                try { cfg = JsonSerializer.Deserialize<MBankAccountConfig>(_bankAccount.BankConfig); }
                catch { cfg = null; }
            }

            if (cfg == null || string.IsNullOrEmpty(cfg.ApiKey) || string.IsNullOrEmpty(cfg.ApiSecret) || string.IsNullOrEmpty(cfg.BillerId))
            {
                result.Status = "SCB_CONFIG_MISSING";
                result.Description = $"Bank account [{_bankAccount.Id}] is missing SCB API config (ApiKey / ApiSecret / BillerId) - กรอกที่หน้า Bank API Config ก่อน";
                return result;
            }

            var baseUrl = cfg.IsSandbox ? SandboxBaseUrl : ProductionBaseUrl;

            string accessToken;
            string tokenType;
            try
            {
                (accessToken, tokenType) = await GetAccessTokenAsync(baseUrl, cfg);
            }
            catch (Exception ex)
            {
                result.Status = "SCB_AUTH_FAILED";
                result.Description = $"Failed to get SCB access token: {ex.Message}";
                return result;
            }

            try
            {
                using var client = new HttpClient();

                var refValue = _pqymentRequest.RefId ?? "";
                //00300100 = Thai QR
                var query = $"eventCode=00300100&transactionDate={Uri.EscapeDataString(transactionDate)}&billerId={Uri.EscapeDataString(cfg.BillerId)}&reference1={Uri.EscapeDataString(refValue)}";
                var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}/v1/payment/billpayment/inquiry?{query}");
                request.Headers.Add("resourceOwnerId", cfg.ApiKey);
                request.Headers.Add("requestUId", Guid.NewGuid().ToString("N"));
                request.Headers.Add("authorization", $"{tokenType} {accessToken}");
                request.Headers.Add("accept-language", "EN");

                var response = await client.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"INFO : [QrGeneratorSCB] billpayment/inquiry response [{(int)response.StatusCode}] : {responseBody}");

                result.RawResponse = responseBody;
                if (!response.IsSuccessStatusCode)
                {
                    result.Status = "SCB_API_ERROR";
                    result.Description = $"SCB billpayment/inquiry failed [{(int)response.StatusCode}]";
                }
            }
            catch (Exception ex)
            {
                result.Status = "SCB_API_ERROR";
                result.Description = $"Failed to call SCB billpayment/inquiry : {ex.Message}";
            }

            return result;
        }
    }

    internal class ScbTokenCache
    {
        public string AccessToken { get; set; } = "";
        public string TokenType { get; set; } = "";
        public long ExpiresAt { get; set; }
    }
}
