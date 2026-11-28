using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.ViewsModels;
using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.Utils;
using System.Text;
using System.Text.Json;
using System.Security.Cryptography;
using System.IdentityModel.Tokens.Jwt;

namespace Its.Onix.Api.Services
{
    public class PortalSessionService : BaseService, IPortalSessionService
    {
        private readonly IPortalSessionRepository? repository = null;
        private readonly IPortalPageService _portalPageService;
        private readonly IPortalRealmService _portalRealmService;
        private readonly IRedisHelper _redis;
        private readonly IHttpClientFactory _httpClientFactory;

        public PortalSessionService(
            IPortalSessionRepository repo,
            IPortalPageService portalPageService,
            IPortalRealmService portalRealmService,
            IRedisHelper redis,
            IHttpClientFactory httpClientFactory) : base()
        {
            repository = repo;
            _portalPageService = portalPageService;
            _portalRealmService = portalRealmService;
            _redis = redis;
            _httpClientFactory = httpClientFactory;
        }

        //ดึงชื่อ realm ของ portal page มาตอน add session (ไม่มี realm ผูกไว้ก็ให้ null ไปเลย)
        private async Task<string?> GetRealmNameForSession(string orgId, Guid? realmId)
        {
            if (realmId == null) return null;

            var realmResult = await _portalRealmService.GetRealmById(orgId, realmId.ToString()!);
            return realmResult.Realm?.RealmName;
        }

        public async Task<MVPortalSession> GetPortalSessions(string orgId, VMPortalSession param)
        {
            var r = new MVPortalSession() { Status = "OK", Description = "Success" };

            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetPortalSessions(param);

            //กัน data ข้าม tenant โผล่ (ข้ามเช็คถ้าเรียกแบบ global)
            if (orgId != "global")
            {
                var mismatch = result.FirstOrDefault(x => x.OrgId != orgId);
                if (mismatch != null)
                {
                    r.Status = "ERROR_DATA_NOT_OWN_BY_ORG_ID";
                    r.Description = $"Portal Session ID [{mismatch.PortalSessionId}] not owned by org [{orgId}] --> [{mismatch.OrgId}]";
                    return await Task.FromResult(r);
                }
            }

            r.PortalSessions = result;
            return await Task.FromResult(r);
        }

        public Task<int> GetPortalSessionCount(string orgId, VMPortalSession param)
        {
            repository!.SetCustomOrgId(orgId);
            return repository!.GetPortalSessionCount(param);
        }

        public async Task<MVPortalSession> GetPortalSessionById(string orgId, string portalSessionId)
        {
            var r = new MVPortalSession() { Status = "OK", Description = "Success" };

            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetPortalSessionById(portalSessionId);

            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Portal Session ID [{portalSessionId}] not found";
                return r;
            }

            if (orgId != "global" && result.OrgId != orgId)
            {
                r.Status = "ERROR_DATA_NOT_OWN_BY_ORG_ID";
                r.Description = $"Portal Session ID [{portalSessionId}] not owned by org [{orgId}] --> [{result.OrgId}]";
                return r;
            }

            r.PortalSession = result;
            return r;
        }

        public async Task<MVPortalSession> UpdatePortalSessionStatusById(string orgId, string portalSessionId, string status, string? statusDesc)
        {
            var r = new MVPortalSession() { Status = "OK", Description = "Success" };

            repository!.SetCustomOrgId(orgId);
            var result = await repository!.UpdatePortalSessionStatusById(portalSessionId, status, statusDesc);

            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Portal Session ID [{portalSessionId}] not found";
                return r;
            }

            r.PortalSession = result;
            return r;
        }

        //เรียกจาก OrgUser-level route แล้ว ผ่าน auth (key) ที่ GenericRbacHandler เช็คให้แล้วตอนนี้ว่ามีสิทธิ์เรียก API นี้จริง
        //apiKeyId คือ key ที่ auth ผ่านมาแล้ว (Temp-Identity-Id) เอามาเช็คแค่ว่าเป็นของ portal page นี้โดยเฉพาะ ไม่ใช่ portal page อื่นในองค์กรเดียวกัน
        public async Task<MVPortalSession> AddPortalSession(string orgId, string? code, string? apiKeyId, Dictionary<string, string> parameters)
        {
            var r = new MVPortalSession() { Status = "OK", Description = "Success" };

            //resolve portal page ตาม code หรือ default ถ้าไม่ระบุ
            MPortalPage? portalPage = !string.IsNullOrEmpty(code)
                ? await _portalPageService.GetPortalPageByCodeForPublic(orgId, code)
                : await _portalPageService.GetDefaultPortalPageForPublic(orgId);

            repository!.SetCustomOrgId(orgId);
            parameters.TryGetValue("mac", out var macAddress);

            if (portalPage == null)
            {
                var failedSession = new MPortalSession()
                {
                    PortalPageCode = code ?? "",
                    MacAddress = macAddress ?? "",
                    ParametersJson = JsonSerializer.Serialize(parameters),
                    Status = "Failed",
                    StatusDesc = $"ERROR_PORTAL_PAGE_NOT_FOUND - Portal page not found for code [{code}] in org [{orgId}]",
                };
                var savedFailed = await repository!.AddPortalSession(failedSession);

                r.PortalSession = savedFailed;
                return r;
            }

            var realmName = await GetRealmNameForSession(orgId, portalPage.RealmId);

            if (string.IsNullOrWhiteSpace(macAddress))
            {
                var noMacSession = new MPortalSession()
                {
                    PortalPageCode = portalPage.Code,
                    AuthMethod = portalPage.AuthMethod,
                    RealmName = realmName,
                    MacAddress = "",
                    ParametersJson = JsonSerializer.Serialize(parameters),
                    Status = "Failed",
                    StatusDesc = "ERROR_MISSING_MAC - MAC address parameter (\"mac\") was not provided",
                };
                var savedNoMac = await repository!.AddPortalSession(noMacSession);

                r.PortalPage = portalPage;
                r.PortalSession = savedNoMac;
                return r;
            }

            if (portalPage.IsEnabled != true)
            {
                var disabledSession = new MPortalSession()
                {
                    PortalPageCode = portalPage.Code,
                    AuthMethod = portalPage.AuthMethod,
                    RealmName = realmName,
                    MacAddress = macAddress ?? "",
                    ParametersJson = JsonSerializer.Serialize(parameters),
                    Status = "Failed",
                    StatusDesc = "ERROR_PORTAL_PAGE_DISABLED - Portal page is disabled",
                };
                var savedDisabled = await repository!.AddPortalSession(disabledSession);

                r.PortalPage = portalPage;
                r.PortalSession = savedDisabled;
                return r;
            }

            var keyStatus = "OK";
            var keyDesc = "Success";

            if (portalPage.ApiKeyId == null || !string.Equals(apiKeyId, portalPage.ApiKeyId.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                keyStatus = "ERROR_API_KEY_NOT_OWN_BY_PORTAL_PAGE";
                keyDesc = "API key does not belong to this portal page";
            }

            var newSession = new MPortalSession()
            {
                PortalPageCode = portalPage.Code,
                AuthMethod = portalPage.AuthMethod,
                RealmName = realmName,
                MacAddress = macAddress ?? "",
                ParametersJson = JsonSerializer.Serialize(parameters),
                Status = keyStatus == "OK" ? "Pending" : "Failed",
                StatusDesc = keyStatus == "OK" ? null : $"{keyStatus} - {keyDesc}",
            };

            var saved = await repository!.AddPortalSession(newSession);

            r.PortalPage = portalPage;
            r.PortalSession = saved;
            return r;
        }

        //คำนวณ URL ที่จะ redirect กลับไปหลัง login สำเร็จ - Direct ใช้ค่าตรงๆ, Parameter อ่านจาก parameter ที่ Mikrotik ส่งมาตอน add session
        private static string? ResolveRedirectUrl(MPortalPage page, string? parametersJson)
        {
            if (string.IsNullOrEmpty(page.RedirectUrlValue)) return null;

            if (page.RedirectUrlMode == "Parameter")
            {
                try
                {
                    var parsed = JsonSerializer.Deserialize<Dictionary<string, string>>(parametersJson ?? "{}");
                    return parsed != null && parsed.TryGetValue(page.RedirectUrlValue, out var v) ? v : null;
                }
                catch
                {
                    return null;
                }
            }

            return page.RedirectUrlValue;
        }

        //เรียกจากหน้า login portal (Native) ตอนผู้ใช้กรอก user/password - OrgUser-level route, auth ด้วย api key ผ่าน Basic auth เหมือน AddPortalSession
        public async Task<MVPortalSession> VerifyNativeLogin(string orgId, string portalSessionId, string userName, string password)
        {
            var r = new MVPortalSession() { Status = "OK", Description = "Success" };

            repository!.SetCustomOrgId(orgId);
            var session = await repository!.GetPortalSessionById(portalSessionId);

            if (session == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Portal Session ID [{portalSessionId}] not found";
                return r;
            }

            var page = !string.IsNullOrEmpty(session.PortalPageCode)
                ? await _portalPageService.GetPortalPageByCodeForPublic(orgId, session.PortalPageCode)
                : null;

            if (page == null || page.RealmId == null)
            {
                var failDesc = "Portal page or realm not configured for this session";
                await repository!.UpdatePortalSessionStatusById(portalSessionId, "Failed", failDesc, userName);

                r.Status = "ERROR_NO_REALM_CONFIGURED";
                r.Description = failDesc;
                return r;
            }

            var loginResult = await _portalRealmService.VerifyRealmUserLogin(orgId, page.RealmId.ToString()!, userName, password);

            if (loginResult.Status != "OK")
            {
                var updatedFailed = await repository!.UpdatePortalSessionStatusById(portalSessionId, "Failed", loginResult.Description, userName);

                r.Status = loginResult.Status;
                r.Description = loginResult.Description;
                r.PortalSession = updatedFailed;
                return r;
            }

            var redirectUrl = ResolveRedirectUrl(page, session.ParametersJson);
            var updatedSession = await repository!.UpdatePortalSessionStatusById(portalSessionId, "Authorized", null, userName);

            r.PortalSession = updatedSession;
            r.RedirectUrl = redirectUrl;
            return r;
        }

        private const string OtpChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

        private static string GenerateOtp(int length = 16)
        {
            var bytes = RandomNumberGenerator.GetBytes(length);
            var sb = new StringBuilder(length);
            foreach (var b in bytes) sb.Append(OtpChars[b % OtpChars.Length]);
            return sb.ToString();
        }

        private static string GenerateNonce() =>
            Convert.ToBase64String(RandomNumberGenerator.GetBytes(24)).Replace('+', '-').Replace('/', '_').TrimEnd('=');

        //ต่อ Google callback URL ให้ตรงกับหน้า realm
        private static string BuildGoogleCallbackUrl(string domain, string orgId) =>
            $"https://{domain.Trim().TrimEnd('/')}/api/OauthCallback/org/{Uri.EscapeDataString(orgId)}/action/GoogleCallback";

        //กดปุ่ม "Login with Google"
        public async Task<MVPortalSession> InitiateGoogleLogin(string orgId, string portalSessionId, string domain)
        {
            var r = new MVPortalSession() { Status = "OK", Description = "Success" };

            if (string.IsNullOrWhiteSpace(domain))
            {
                r.Status = "ERROR_VALIDATION_DOMAIN";
                r.Description = "domain is required";
                return r;
            }

            var redirectUri = BuildGoogleCallbackUrl(domain, orgId);

            repository!.SetCustomOrgId(orgId);
            var session = await repository!.GetPortalSessionById(portalSessionId);

            if (session == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Portal Session ID [{portalSessionId}] not found";
                return r;
            }

            var page = !string.IsNullOrEmpty(session.PortalPageCode)
                ? await _portalPageService.GetPortalPageByCodeForPublic(orgId, session.PortalPageCode)
                : null;

            if (page == null || page.RealmId == null)
            {
                var failDesc = "Portal page or realm not configured for this session";
                await repository!.UpdatePortalSessionStatusById(portalSessionId, "Failed", failDesc);

                r.Status = "ERROR_NO_REALM_CONFIGURED";
                r.Description = failDesc;
                return r;
            }

            var realmResult = await _portalRealmService.GetRealmById(orgId, page.RealmId.ToString()!);
            var realm = realmResult.Realm;

            if (realm == null || realm.AuthType != "Google" || string.IsNullOrEmpty(realm.GoogleClientId) || string.IsNullOrEmpty(realm.GoogleClientSecret))
            {
                var failDesc = "Realm is not configured for Google authentication";
                await repository!.UpdatePortalSessionStatusById(portalSessionId, "Failed", failDesc);

                r.Status = "ERROR_REALM_NOT_GOOGLE";
                r.Description = failDesc;
                return r;
            }

            var otp = GenerateOtp();
            var nonce = GenerateNonce();

            var oauthState = new MVGoogleOAuthState
            {
                OrgId = orgId,
                PortalSessionId = portalSessionId,
                ClientId = realm.GoogleClientId,
                ClientSecret = realm.GoogleClientSecret,
                RedirectUri = redirectUri,
                Nonce = nonce,
            };

            //TTL 1 ชม., key = OTP (state)
            await _redis.SetObjectAsync($"GoogleOAuth:{otp}", oauthState, TimeSpan.FromHours(1));

            var qs = $"client_id={Uri.EscapeDataString(realm.GoogleClientId)}" +
                     $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
                     "&response_type=code" +
                     $"&scope={Uri.EscapeDataString("openid email profile")}" +
                     $"&state={Uri.EscapeDataString(otp)}" +
                     $"&nonce={Uri.EscapeDataString(nonce)}";

            r.GoogleAuthUrl = $"https://accounts.google.com/o/oauth2/v2/auth?{qs}";
            return r;
        }

        //Google redirect กลับมาเรียกตรงนี้ (public)
        public async Task<MVPortalSession> HandleGoogleCallback(string orgId, string? code, string? state, string? error)
        {
            var r = new MVPortalSession() { Status = "OK", Description = "Success" };

            repository!.SetCustomOrgId(orgId);

            if (string.IsNullOrEmpty(state))
            {
                r.Status = "ERROR_MISSING_STATE";
                r.Description = "Missing state parameter";
                return r;
            }

            var redisKey = $"GoogleOAuth:{state}";
            var oauthState = await _redis.GetObjectAsync<MVGoogleOAuthState>(redisKey);

            if (oauthState == null)
            {
                r.Status = "ERROR_INVALID_OR_EXPIRED_STATE";
                r.Description = "Google login request not found or expired, please try signing in again";
                return r;
            }

            //one-time use ลบทันที
            await _redis.DeleteAsync(redisKey);

            if (!string.Equals(oauthState.OrgId, orgId, StringComparison.Ordinal))
            {
                if (!string.IsNullOrEmpty(oauthState.PortalSessionId))
                {
                    await repository!.UpdatePortalSessionStatusById(oauthState.PortalSessionId, "Failed", "ERROR_ORG_MISMATCH - state does not belong to this org");
                }

                r.Status = "ERROR_ORG_MISMATCH";
                r.Description = "state does not belong to this organization";
                return r;
            }

            if (!string.IsNullOrEmpty(error))
            {
                await repository!.UpdatePortalSessionStatusById(oauthState.PortalSessionId!, "Failed", $"GOOGLE_OAUTH_DENIED - {error}");

                r.Status = "GOOGLE_OAUTH_DENIED";
                r.Description = $"Google sign-in was not completed: {error}";
                return r;
            }

            if (string.IsNullOrEmpty(code))
            {
                await repository!.UpdatePortalSessionStatusById(oauthState.PortalSessionId!, "Failed", "ERROR_MISSING_CODE - Google did not return an authorization code");

                r.Status = "ERROR_MISSING_CODE";
                r.Description = "Google did not return an authorization code";
                return r;
            }

            var session = await repository!.GetPortalSessionById(oauthState.PortalSessionId!);
            if (session == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Portal Session ID [{oauthState.PortalSessionId}] not found";
                return r;
            }

            var page = !string.IsNullOrEmpty(session.PortalPageCode)
                ? await _portalPageService.GetPortalPageByCodeForPublic(orgId, session.PortalPageCode)
                : null;

            string? idToken;
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var tokenRequestBody = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["code"] = code,
                    ["client_id"] = oauthState.ClientId ?? "",
                    ["client_secret"] = oauthState.ClientSecret ?? "",
                    ["redirect_uri"] = oauthState.RedirectUri ?? "",
                    ["grant_type"] = "authorization_code",
                });

                var tokenResponse = await httpClient.PostAsync("https://oauth2.googleapis.com/token", tokenRequestBody);
                var tokenJson = await tokenResponse.Content.ReadAsStringAsync();

                if (!tokenResponse.IsSuccessStatusCode)
                {
                    await repository!.UpdatePortalSessionStatusById(oauthState.PortalSessionId!, "Failed", $"ERROR_GOOGLE_TOKEN_EXCHANGE - {tokenJson}");

                    r.Status = "ERROR_GOOGLE_TOKEN_EXCHANGE";
                    r.Description = "Failed to exchange authorization code with Google";
                    return r;
                }

                using var doc = JsonDocument.Parse(tokenJson);
                idToken = doc.RootElement.TryGetProperty("id_token", out var idTokenEl) ? idTokenEl.GetString() : null;
            }
            catch (Exception ex)
            {
                await repository!.UpdatePortalSessionStatusById(oauthState.PortalSessionId!, "Failed", $"ERROR_GOOGLE_TOKEN_EXCHANGE - {ex.Message}");

                r.Status = "ERROR_GOOGLE_TOKEN_EXCHANGE";
                r.Description = "Failed to exchange authorization code with Google";
                return r;
            }

            if (string.IsNullOrEmpty(idToken))
            {
                await repository!.UpdatePortalSessionStatusById(oauthState.PortalSessionId!, "Failed", "ERROR_GOOGLE_NO_ID_TOKEN - Google did not return an id_token");

                r.Status = "ERROR_GOOGLE_NO_ID_TOKEN";
                r.Description = "Google did not return an id_token";
                return r;
            }

            JwtSecurityToken jwt;
            try
            {
                jwt = new JwtSecurityTokenHandler().ReadJwtToken(idToken);
            }
            catch (Exception)
            {
                await repository!.UpdatePortalSessionStatusById(oauthState.PortalSessionId!, "Failed", "ERROR_GOOGLE_INVALID_ID_TOKEN - Could not parse id_token");

                r.Status = "ERROR_GOOGLE_INVALID_ID_TOKEN";
                r.Description = "Could not parse Google's id_token";
                return r;
            }

            //เช็ค nonce กัน replay
            var nonceClaim = jwt.Claims.FirstOrDefault(c => c.Type == "nonce")?.Value;
            if (!string.Equals(nonceClaim, oauthState.Nonce, StringComparison.Ordinal))
            {
                await repository!.UpdatePortalSessionStatusById(oauthState.PortalSessionId!, "Failed", "ERROR_NONCE_MISMATCH - nonce in Google's id_token does not match");

                r.Status = "ERROR_NONCE_MISMATCH";
                r.Description = "Security check failed (nonce mismatch), please try signing in again";
                return r;
            }

            var email = jwt.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
            var claimsJson = JsonSerializer.Serialize(jwt.Claims.ToDictionary(c => c.Type, c => c.Value));

            var redirectUrl = page != null ? ResolveRedirectUrl(page, session.ParametersJson) : null;
            var updatedSession = await repository!.UpdatePortalSessionStatusById(oauthState.PortalSessionId!, "Authorized", null, email, claimsJson);

            r.PortalSession = updatedSession;
            r.RedirectUrl = redirectUrl;
            return r;
        }
    }
}
