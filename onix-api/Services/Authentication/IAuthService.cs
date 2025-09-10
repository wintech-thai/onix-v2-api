namespace Its.Onix.Api.Services
{
    public interface IAuthService
    {
        public UserToken Login(UserLogin userLogin);
        public UserToken RefreshToken(string token);
    }
}
