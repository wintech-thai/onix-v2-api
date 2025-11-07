using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Services
{
    public interface IAdminUserService
    {
        public Task<IEnumerable<MAdminUser>> GetUsers(VMAdminUser param);
        public Task<int> GetUserCount(VMAdminUser param);
        public Task<MVAdminUser> GetUserById(string userId);
        public Task<MVAdminUser?> DeleteUserById(string userId);
        public Task<MVAdminUser?> UpdateUserById(string userId, MAdminUser user);
        public Task<MVAdminUser?> UpdateUserStatusById(string userId, string status);
        public Task<MVAdminUser?> UpdateUserStatusById(string adminUserId, string userId, string status);
        public Task<MVAdminUser?> InviteUser(MAdminUser user);
        public MVAdminUser VerifyUserIsAdmin(string userName);
    }
}
