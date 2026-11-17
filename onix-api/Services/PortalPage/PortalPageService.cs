using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.ViewsModels;
using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.Utils;

namespace Its.Onix.Api.Services
{
    public class PortalPageService : BaseService, IPortalPageService
    {
        private readonly IPortalPageRepository? repository = null;
        private readonly IApiKeyService _apiKeyService;

        public PortalPageService(
            IPortalPageRepository repo,
            IApiKeyService apiKeyService) : base()
        {
            repository = repo;
            _apiKeyService = apiKeyService;
        }

        public async Task<IEnumerable<MPortalPage>> GetPortalPages(string orgId, VMPortalPage param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetPortalPages(param);

            return result;
        }

        public Task<int> GetPortalPageCount(string orgId, VMPortalPage param)
        {
            repository!.SetCustomOrgId(orgId);
            return repository!.GetPortalPageCount(param);
        }

        public async Task<MVPortalPage> GetPortalPageById(string orgId, string portalPageId)
        {
            var r = new MVPortalPage() { Status = "OK", Description = "Success" };

            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetPortalPageById(portalPageId);

            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Portal Page ID [{portalPageId}] not found";
                return r;
            }

            //กัน data ข้าม tenant โผล่ (global ข้ามเช็คนี้)
            if (orgId != "global" && result.OrgId != orgId)
            {
                r.Status = "ERROR_DATA_NOT_OWN_BY_ORG_ID";
                r.Description = $"Portal Page ID [{portalPageId}] not owned by org [{orgId}]";
                return r;
            }

            r.PortalPage = result;
            return r;
        }

        public async Task<MVPortalPage> AddPortalPage(string orgId, MPortalPage portalPage)
        {
            var r = new MVPortalPage() { Status = "OK", Description = "Success" };

            var codeValidation = ValidationUtils.ValidatePortalPageCode(portalPage.Code ?? "");
            if (codeValidation.Status != "OK")
            {
                r.Status = codeValidation.Status!;
                r.Description = codeValidation.Description!;
                return r;
            }

            repository!.SetCustomOrgId(orgId);

            var existing = await repository!.GetPortalPageByCode(portalPage.Code!);
            if (existing != null)
            {
                r.Status = "CODE_DUPLICATE";
                r.Description = $"Portal page code [{portalPage.Code}] is already used in this organization";
                return r;
            }

            var result = await repository!.AddPortalPage(portalPage);

            r.PortalPage = result;
            return r;
        }

        public async Task<MVPortalPage> UpdatePortalPageById(string orgId, string portalPageId, MPortalPage portalPage)
        {
            var r = new MVPortalPage() { Status = "OK", Description = "Success" };

            var codeValidation = ValidationUtils.ValidatePortalPageCode(portalPage.Code ?? "");
            if (codeValidation.Status != "OK")
            {
                r.Status = codeValidation.Status!;
                r.Description = codeValidation.Description!;
                return r;
            }

            repository!.SetCustomOrgId(orgId);

            var existing = await repository!.GetPortalPageByCode(portalPage.Code!);
            if (existing != null && existing.PortalPageId.ToString() != portalPageId)
            {
                r.Status = "CODE_DUPLICATE";
                r.Description = $"Portal page code [{portalPage.Code}] is already used in this organization";
                return r;
            }

            var result = await repository!.UpdatePortalPageById(portalPageId, portalPage);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Portal Page ID [{portalPageId}] not found";
                return r;
            }

            r.PortalPage = result;
            return r;
        }

        public async Task<MVPortalPage> DeletePortalPageById(string orgId, string portalPageId)
        {
            var r = new MVPortalPage() { Status = "OK", Description = "Success" };

            repository!.SetCustomOrgId(orgId);
            var result = await repository!.DeletePortalPageById(portalPageId);

            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Portal Page ID [{portalPageId}] not found";
                return r;
            }

            r.PortalPage = result;
            return r;
        }

        public async Task<MVPortalPage> UpdatePortalPageStatusById(string orgId, string portalPageId, bool isEnabled)
        {
            var r = new MVPortalPage() { Status = "OK", Description = "Success" };

            repository!.SetCustomOrgId(orgId);
            var result = await repository!.UpdatePortalPageStatusById(portalPageId, isEnabled);

            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Portal Page ID [{portalPageId}] not found";
                return r;
            }

            r.PortalPage = result;
            return r;
        }

        public async Task<MVPortalPage> SetDefaultPortalPageById(string orgId, string portalPageId)
        {
            var r = new MVPortalPage() { Status = "OK", Description = "Success" };

            repository!.SetCustomOrgId(orgId);
            var result = await repository!.SetDefaultPortalPageById(portalPageId);

            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Portal Page ID [{portalPageId}] not found";
                return r;
            }

            r.PortalPage = result;
            return r;
        }

        public async Task<MVApiKey> CreatePortalPageApiKey(string orgId, string portalPageId, string keyName, string? keyDescription)
        {
            var r = new MVApiKey() { Status = "OK", Description = "Success" };

            repository!.SetCustomOrgId(orgId);
            var portalPage = await repository!.GetPortalPageById(portalPageId);
            if (portalPage == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Portal Page ID [{portalPageId}] not found";
                return r;
            }

            //key_name unique ข้าม org ทั้งระบบ ต้องกันชนเอง
            var uniqueKeyName = $"portal-{orgId}-{portalPage.Code}-{Guid.NewGuid().ToString("N")[..6]}";
            if (!string.IsNullOrEmpty(keyName))
            {
                uniqueKeyName = $"{keyName}-{Guid.NewGuid().ToString("N")[..6]}";
            }

            var apiKey = new MApiKey()
            {
                KeyName = uniqueKeyName,
                KeyDescription = keyDescription ?? $"API key for portal page [{portalPage.Code}]",
                Roles = ["PORTAL_PAGE"],
            };

            var addResult = _apiKeyService.AddApiKey(orgId, apiKey);
            if (addResult == null || addResult.Status != "OK")
            {
                r.Status = addResult?.Status ?? "ERROR_CREATE_API_KEY";
                r.Description = addResult?.Description ?? "Failed to create API key for portal page";
                return r;
            }

            //AddApiKey ไม่ persist RolesList ให้ ต้องตามด้วย UpdateApiKeyById เหมือน api-keys/create ฝั่ง frontend
            _apiKeyService.UpdateApiKeyById(orgId, addResult.ApiKey!.KeyId!.Value.ToString(), new MApiKey { Roles = ["PORTAL_PAGE"] });

            await repository!.UpdatePortalPageApiKeyById(portalPageId, addResult.ApiKey!.KeyId!.Value, uniqueKeyName);

            r.ApiKey = addResult.ApiKey;
            return r;
        }

        public async Task<MPortalPage?> GetPortalPageByCodeForPublic(string orgId, string code)
        {
            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetPortalPageByCode(code);

            return result;
        }

        public async Task<MPortalPage?> GetDefaultPortalPageForPublic(string orgId)
        {
            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetDefaultPortalPage();

            return result;
        }
    }
}
