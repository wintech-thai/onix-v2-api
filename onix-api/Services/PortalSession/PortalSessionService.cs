using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.ViewsModels;
using Its.Onix.Api.Database.Repositories;
using System.Text.Json;

namespace Its.Onix.Api.Services
{
    public class PortalSessionService : BaseService, IPortalSessionService
    {
        private readonly IPortalSessionRepository? repository = null;
        private readonly IPortalPageService _portalPageService;
        private readonly IPortalRealmService _portalRealmService;

        public PortalSessionService(
            IPortalSessionRepository repo,
            IPortalPageService portalPageService,
            IPortalRealmService portalRealmService) : base()
        {
            repository = repo;
            _portalPageService = portalPageService;
            _portalRealmService = portalRealmService;
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
                    StatusDesc = $"Portal page not found for code [{code}] in org [{orgId}]",
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
                    StatusDesc = "MAC address parameter (\"mac\") was not provided",
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
                    StatusDesc = "Portal page is disabled",
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
    }
}
