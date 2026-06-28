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

                //ref1 ใช้ RefId ตัวเดียวกันกับที่สร้าง payment request ไว้ เพื่อให้เอาไป matching ตอน SCB ยิง payment confirmation webhook กลับมาได้
                var refValue = _pqymentRequest.RefId ?? Guid.NewGuid().ToString("N");
                //ref3 = Ref3Prefix + ค่า ตาม format ที่ SCB กำหนด (เช่น "SCB1234") - Ref3Prefix ได้มาจาก Merchant Profile ของ SCB
                var ref3 = $"{cfg.Ref3Prefix}{refValue}";
                //Biller ID นี้ตั้ง Supporting Reference เป็น "Two references" ไว้ที่ Merchant Profile ของ SCB ดังนั้น ref2 ต้องมีค่าเสมอ (ไม่ใช่ optional)
                //ถ้าผู้ใช้ไม่ได้กรอก RefId1 มาจาก form (REF1 ใน QrPaymentModal) ให้ fallback ไปใช้ refValue ซ้ำกัน เพื่อไม่ให้ส่ง ref2 เป็นค่าว่าง
                var ref2Value = !string.IsNullOrWhiteSpace(_pqymentRequest.RefId1) ? _pqymentRequest.RefId1 : refValue;

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

        //ดึง JWT token จาก Redis ถ้ายังไม่ expire, ถ้าไม่มีหรือ expire แล้วก็ขอใหม่จาก SCB /v1/oauth/token แล้วเก็บกลับเข้า Redis
        //เก็บ cache แยกตาม org + bank account เพื่อให้ request อื่น ๆ ที่เรียกบัญชีเดียวกันแชร์ token ร่วมกันได้ ไม่ต้อง authen ซ้ำจนติด rate limit
        private async Task<(string accessToken, string tokenType)> GetAccessTokenAsync(string baseUrl, MBankAccountConfig cfg)
        {
            var cacheKey = $"{CacheHelper.CreateBankApiTokenKey(_bankAccount.OrgId ?? "global", "SCBToken")}:{_bankAccount.Id}";

            var cached = await _redis.GetObjectAsync<ScbTokenCache>(cacheKey);
            var nowEpoch = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (cached != null && cached.ExpiresAt > nowEpoch + 10) //เผื่อ buffer 10 วินาทีก่อนหมดอายุจริง กัน race condition
            {
                return (cached.AccessToken, cached.TokenType);
            }

            using var client = new HttpClient();

            //ใช้ client credentials แบบ applicationKey/applicationSecret (ไม่ใช่ authorization_code flow เพราะเราไม่ได้เข้าถึงข้อมูลเฉพาะของ user)
            //ตาม schema ของ SCB เคสนี้จะไม่ได้ refreshToken กลับมา (refreshToken คืนค่าเฉพาะ authorization_code grant เท่านั้น)
            //ดังนั้นเวลา token หมดอายุ ให้ขอ token ใหม่จาก /v1/oauth/token ตรงนี้ซ้ำเลย ไม่ต้องเรียก /v1/oauth/token/refresh
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

        //ใช้เช็คสถานะ payment เองโดยไม่ต้องพึ่ง payment confirmation webhook จาก SCB
        //อ้างอิงจาก https://developer.scb/#/documents/api-reference-index/qr-payments/get-billpayment-inquiry.html - "Recommended use case is when
        //customer has informed partner that they have been paid successfully, but partner did not receive payment confirmation from SCB"
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
                //eventCode "00300100" = Thai QR (ตรงกับ QR30/BILLERID ที่เราใช้ตอนสร้าง QR)
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
