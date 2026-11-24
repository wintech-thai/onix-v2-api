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
        private readonly ICustomRoleService _customRoleService;

        public PortalPageService(
            IPortalPageRepository repo,
            IApiKeyService apiKeyService,
            ICustomRoleService customRoleService) : base()
        {
            repository = repo;
            _apiKeyService = apiKeyService;
            _customRoleService = customRoleService;
        }

        //ให้สิทธิ์เรียก AddPortalSession ผ่าน custom role จริงที่ auth pipeline รู้จัก แทนการแปะ role string ปลอมๆ ที่ระบบไม่เคยรู้จัก
        //เรียกทั้งตอนสร้าง key ใหม่ และตอนเจอ key เก่าที่ยังไม่เคยมี custom role (migrate key เก่าแบบ lazy)
        private async Task<MApiKey> EnsureCustomRoleAssigned(string orgId, MPortalPage portalPage, MApiKey apiKey)
        {
            if (!string.IsNullOrEmpty(apiKey.CustomRoleId)) return apiKey;

            var customRole = new MCustomRole()
            {
                RoleName = $"portal-page-{apiKey.KeyName}",
                RoleDescription = $"Allows calling AddPortalSession for portal page [{portalPage.Code}]",
                Level = "UserRole",
                Permissions =
                [
                    new ControllerNode()
                    {
                        ControllerName = "PortalSession",
                        ApiPermissions = [ new ApiNode() { ControllerName = "PortalSession", ApiName = "AddPortalSession", IsAllowed = true } ],
                    }
                ],
            };
            var roleResult = await _customRoleService.AddCustomRole(orgId, customRole);

            _apiKeyService.UpdateApiKeyById(orgId, apiKey.KeyId!.Value.ToString(), new MApiKey()
            {
                KeyDescription = apiKey.KeyDescription,
                CustomRoleId = roleResult.CustomRole!.RoleId.ToString(),
            });

            apiKey.CustomRoleId = roleResult.CustomRole!.RoleId.ToString();
            apiKey.CustomRoleName = customRole.RoleName;
            return apiKey;
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

            //ไม่ระบุภาษามาให้มองเป็น Thai
            portalPage.Language = string.IsNullOrEmpty(portalPage.Language) ? "Thai" : portalPage.Language;

            var result = await repository!.AddPortalPage(portalPage);

            //สร้าง api key + custom role ผูกไว้ตั้งแต่ตอนสร้าง portal page เลย (ของเดิมไปสร้างตอน get link)
            //reuse CreatePortalPageApiKey ตัวเดียวกับที่ "get link" ใช้ เพื่อไม่ให้ logic การสร้าง key+role มีสองที่
            var apiKeyResult = await CreatePortalPageApiKey(orgId, result.PortalPageId!.Value.ToString(), "", null);
            if (apiKeyResult.Status == "OK" && apiKeyResult.ApiKey != null)
            {
                result.ApiKeyId = apiKeyResult.ApiKey.KeyId;
                result.ApiCode = apiKeyResult.ApiKey.KeyName;
            }

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

            //ไม่ระบุภาษามาให้มองเป็น Thai
            portalPage.Language = string.IsNullOrEmpty(portalPage.Language) ? "Thai" : portalPage.Language;

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
            };

            var addResult = _apiKeyService.AddApiKey(orgId, apiKey);
            if (addResult == null || addResult.Status != "OK")
            {
                r.Status = addResult?.Status ?? "ERROR_CREATE_API_KEY";
                r.Description = addResult?.Description ?? "Failed to create API key for portal page";
                return r;
            }

            await repository!.UpdatePortalPageApiKeyById(portalPageId, addResult.ApiKey!.KeyId!.Value, uniqueKeyName);

            r.ApiKey = await EnsureCustomRoleAssigned(orgId, portalPage, addResult.ApiKey!);
            return r;
        }

        public async Task<MVApiKey> GetPortalPageApiKey(string orgId, string portalPageId, string keyName, string? keyDescription)
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

            if (portalPage.ApiKeyId != null)
            {
                var existingKey = await _apiKeyService.GetApiKeyRawById(orgId, portalPage.ApiKeyId.ToString()!);
                if (existingKey != null)
                {
                    r.ApiKey = await EnsureCustomRoleAssigned(orgId, portalPage, existingKey);
                    return r;
                }
                //ApiKeyId มีแต่หา key จริงไม่เจอ (เช่นถูกลบไปที่หน้า Api Keys) ตกไปสร้างใหม่แทนด้านล่าง
            }

            return await CreatePortalPageApiKey(orgId, portalPageId, keyName, keyDescription);
        }

        public async Task<MPortalPage?> GetPortalPageByCodeForPublic(string orgId, string code)
        {
            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetPortalPageByCode(code);

            if (result != null) result.Language ??= "Thai"; //ของเก่า null/empty ให้มองเป็น Thai เสมอ
            return result;
        }

        public async Task<MPortalPage?> GetDefaultPortalPageForPublic(string orgId)
        {
            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetDefaultPortalPage();

            if (result != null) result.Language ??= "Thai"; //ของเก่า null/empty ให้มองเป็น Thai เสมอ
            return result;
        }
    }
}
