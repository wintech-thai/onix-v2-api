using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Database.Repositories
{
    public interface IAdminUserRepository
    {
        public Task<MAdminUser> GetUserById(string userId);
        public Task<MAdminUser> GetUserByName(string userName);
        public Task<MAdminUser> GetUserByIdLeftJoin(string userId);
        public Task<MAdminUser> AddUser(MAdminUser user);
        public Task<MAdminUser?> DeleteUserById(string userId);
        public Task<IEnumerable<MAdminUser>> GetUsersLeftJoin(VMAdminUser param);
        public Task<int> GetUserCountLeftJoin(VMAdminUser param);
        public Task<MAdminUser?> UpdateUserById(MAdminUser user);
        public Task<MAdminUser?> UpdateUserStatusById(string userId, string status);
        public Task<MAdminUser?> UpdateUserStatusById(string adminUserId, string userId, string status);
        public Task<bool> IsUserNameExist(string userName);
    }
}
