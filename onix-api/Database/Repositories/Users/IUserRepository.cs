using Its.Onix.Api.Models;

namespace Its.Onix.Api.Database.Repositories
{
    public interface IUserRepository
    {
        public void SetCustomOrgId(string customOrgId);
        public MUser AddUser(MUser user);
        public IEnumerable<MUser> GetUsers();

        public bool IsEmailExist(string email);
        public bool IsUserNameExist(string userName);
        public bool IsUserIdExist(string userId);

        public MUser GetUserByName(string userName);
        public MUser GetUserByEmail(string email);
    }
}
