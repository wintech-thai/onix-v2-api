using Its.Onix.Api.Models;
using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.ViewsModels;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.Utils;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using System.Text.Json;

namespace Its.Onix.Api.Services
{
    public class CustomRoleService : BaseService, ICustomRoleService
    {
        private readonly ICustomRoleRepository? repository = null;
        private readonly IApiDescriptionGroupCollectionProvider _provider;

        public CustomRoleService(ICustomRoleRepository repo, IApiDescriptionGroupCollectionProvider provider) : base()
        {
            repository = repo;
            _provider = provider;
        }

        public async Task<MVCustomRole> GetCustomRoleById(string orgId, string customRoleId)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVCustomRole()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(customRoleId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Custom role ID [{customRoleId}] format is invalid";

                return r;
            }

            var result = await repository!.GetCustomRoleById(customRoleId);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Custom role ID [{customRoleId}] not found for the organization [{orgId}]";

                return r;
            }

            var roleDef = result.RoleDefinition;
            if (string.IsNullOrEmpty(roleDef))
            {
                roleDef = "[]";
            }

            var perms = JsonSerializer.Deserialize<List<ControllerNode>>(roleDef);
            var flatPermissions = new Dictionary<string, bool>();

            if (perms != null)
            {
                flatPermissions = GetFlatenPermission(perms);
            }

            var permissions = GetInitialPermission("api", flatPermissions);

            r.CustomRole = result;
            r.CustomRole.Permissions = permissions;
            r.CustomRole.RoleDefinition = "";

            return r;
        }

        private Dictionary<string, bool> GetFlatenPermission(List<ControllerNode> controlers)
        {
            var flattenMap = new Dictionary<string, bool>();

            foreach (var ctrl in controlers)
            {
                foreach (var permission in ctrl.ApiPermissions)
                {
                    var key = $"{permission.ControllerName}:{permission.ApiName}";
                    flattenMap.Add(key, permission.IsAllowed);
                }
            }

            return flattenMap;
        }

        public async Task<MVCustomRole> AddCustomRole(string orgId, MCustomRole customRole)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVCustomRole();
            r.Status = "OK";
            r.Description = "Success";

            if (string.IsNullOrEmpty(customRole.RoleName))
            {
                r.Status = "NAME_MISSING";
                r.Description = $"Custom role name is missing!!!";

                return r;
            }

            var isExist = await repository!.IsRoleNameExist(customRole.RoleName);
            if (isExist)
            {
                r.Status = "NAME_DUPLICATE";
                r.Description = $"Custom role name [{customRole.RoleName}] already exist!!!";

                return r;
            }

            customRole.Permissions ??= []; //Empty array
            customRole.RoleDefinition = JsonSerializer.Serialize(customRole.Permissions)!;

            var result = await repository!.AddCustomRole(customRole);
            r.CustomRole = result;

            //ไม่ให้ส่งออกไป แต่เช็คเพิ่มเติมนะว่าไม่ได้ update กลับไปที่ DB
            r.CustomRole.RoleDefinition = "";

            return r;
        }

        public async Task<MVCustomRole> DeleteCustomRoleById(string orgId, string customRoleId)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVCustomRole()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(customRoleId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Custom role ID [{customRoleId}] format is invalid";

                return r;
            }

            var m = await repository!.DeleteCustomRoleById(customRoleId);
            if (m == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Custom role ID [{customRoleId}] not found for the organization [{orgId}]";

                return r;
            }

            r.CustomRole = m;
            return r;
        }

        public async Task<List<MCustomRole>> GetCustomRoles(string orgId, VMCustomRole param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetCustomRoles(param);

            return result;
        }

        public async Task<int> GetCustomRoleCount(string orgId, VMCustomRole param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetCustomRoleCount(param);

            return result;
        }

        public async Task<MVCustomRole> UpdateCustomRoleById(string orgId, string customRoleId, MCustomRole customRole)
        {
            //TODO : Check if custom role name is duplicate
            repository!.SetCustomOrgId(orgId);

            var r = new MVCustomRole()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(customRoleId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Custom role ID [{customRoleId}] format is invalid";

                return r;
            }

            customRole.Permissions ??= []; //Empty array
            customRole.RoleDefinition = JsonSerializer.Serialize(customRole.Permissions)!;

            var result = await repository!.UpdateCustomRoleById(customRoleId, customRole);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Custom role ID [{customRoleId}] not found for the organization [{orgId}]";

                return r;
            }

            r.CustomRole = result;
            //ไม่ให้ส่งออกไป แต่เช็คเพิ่มเติมนะว่าไม่ได้ update กลับไปที่ DB
            r.CustomRole.RoleDefinition = "";

            return r;
        }

        private List<ControllerNode> GetInitialPermission(string filterApiGroup, Dictionary<string, bool> flatPermissions)
        {
            var controllers = new List<ControllerNode>();
            var controlerMap = new Dictionary<string, ControllerNode>();

            foreach (var group in _provider.ApiDescriptionGroups.Items)
            {
                foreach (var api in group.Items)
                {
                    var controller = api.ActionDescriptor.RouteValues["controller"]!;
                    var action = api.ActionDescriptor.RouteValues["action"];
                    var route = api.RelativePath!;
                    string apiGroup = route.Split("/", StringSplitOptions.RemoveEmptyEntries)[0];

                    if (apiGroup != filterApiGroup)
                    {
                        continue;
                    }

                    ControllerNode ctrlNode;
                    if (controlerMap.ContainsKey(controller))
                    {
                        ctrlNode = controlerMap[controller];
                    }
                    else
                    {
                        ctrlNode = new ControllerNode();
                        ctrlNode.ControllerName = controller;

                        controlerMap.Add(controller, ctrlNode);
                        controllers.Add(ctrlNode);
                    }

                    var key = $"{controller}:{action}";
                    var isSelected = false;
                    if (flatPermissions.TryGetValue(key, out bool value))
                    {
                        isSelected = value;
                    }

                    var apiNode = new ApiNode()
                    {
                        ApiName = action!,
                        ControllerName = controller,
                        IsAllowed  = isSelected,
                    };

                    ctrlNode.ApiPermissions.Add(apiNode);
                }
            }

            return controllers;
        }

        public MVCustomPermission GetInitialUserRolePermissions(string orgId)
        {
            var r = new MVCustomPermission()
            {
                Status = "OK",
                Description = "Success"
            };

            var permissions = GetInitialPermission("api", []);

            r.Permissions = permissions;

            return r;
        }
    }
}
