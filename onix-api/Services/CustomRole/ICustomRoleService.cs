using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Services
{
    public interface ICustomRoleService
    {
        public Task<MVCustomRole> GetCustomRoleById(string orgId, string customRoleId);
        public Task<MVCustomRole> AddCustomRole(string orgId, MCustomRole customRole);
        public Task<MVCustomRole> DeleteCustomRoleById(string orgId, string customRoleId);
        public Task<List<MCustomRole>> GetCustomRoles(string orgId, VMCustomRole param);
        public Task<int> GetCustomRoleCount(string orgId, VMCustomRole param);
        public Task<MVCustomRole> UpdateCustomRoleById(string orgId, string customRoleId, MCustomRole customRole);
        public MVCustomPermission GetInitialUserRolePermissions(string orgId); /* สำหรับ Level ที่เป็น API ของ User (console) */
    }
}
