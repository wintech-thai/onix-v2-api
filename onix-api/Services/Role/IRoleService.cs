using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Services
{
    public interface IRoleService
    {
        public IEnumerable<MRole> GetRolesList(string orgId, string rolesList);
        public IEnumerable<MRole> GetRoles(string orgId, VMRole param);
    }
}
