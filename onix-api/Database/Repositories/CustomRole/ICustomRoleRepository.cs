using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Database.Repositories
{
    public interface ICustomRoleRepository
    {
        public void SetCustomOrgId(string customOrgId);

        public Task<bool> IsRoleNameExist(string roleName);
        public Task<List<MCustomRole>> GetCustomRoles(VMCustomRole param);
        public Task<int> GetCustomRoleCount(VMCustomRole param);
        public Task<MCustomRole?> GetCustomRoleById(string customRoleId);
        public Task<MCustomRole> AddCustomRole(MCustomRole customRole);
        public Task<MCustomRole?> DeleteCustomRoleById(string customRoleId);
        public Task<MCustomRole?> UpdateCustomRoleById(string customRoleId, MCustomRole customRole);


/*
        public Task<MScanItemAction?> SetScanItemActionDefault_V2(string actionId);
*/
    }
}
