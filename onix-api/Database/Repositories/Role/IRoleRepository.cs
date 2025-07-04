using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Database.Repositories
{
    public interface IRoleRepository
    {
        public void SetCustomOrgId(string customOrgId);
        public IEnumerable<MRole> GetRolesList(string rolesList);
        public IEnumerable<MRole> GetRoles(VMRole param);
    }
}
