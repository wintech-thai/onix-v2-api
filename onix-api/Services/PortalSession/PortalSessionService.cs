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

        public PortalSessionService(
            IPortalSessionRepository repo,
            IPortalPageService portalPageService) : base()
        {
            repository = repo;
            _portalPageService = portalPageService;
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

            if (portalPage == null)
            {
                var failedSession = new MPortalSession()
                {
                    PortalPageCode = code ?? "",
                    ParametersJson = JsonSerializer.Serialize(parameters),
                    Status = "Failed",
                    StatusDesc = $"Portal page not found for code [{code}] in org [{orgId}]",
                };
                var savedFailed = await repository!.AddPortalSession(failedSession);

                r.PortalSession = savedFailed;
                return r;
            }

            if (portalPage.IsEnabled != true)
            {
                var disabledSession = new MPortalSession()
                {
                    PortalPageCode = portalPage.Code,
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
                ParametersJson = JsonSerializer.Serialize(parameters),
                Status = keyStatus == "OK" ? "Pending" : "Failed",
                StatusDesc = keyStatus == "OK" ? null : $"{keyStatus} - {keyDesc}",
            };

            var saved = await repository!.AddPortalSession(newSession);

            r.PortalPage = portalPage;
            r.PortalSession = saved;
            return r;
        }
    }
}
