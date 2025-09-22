using System.IdentityModel.Tokens.Jwt;
using Its.Onix.Api.Models;
using Microsoft.IdentityModel.Tokens;

namespace Its.Onix.Api.Services
{
    public interface IAuthService
    {
        public UserToken Login(UserLogin userLogin);
        public UserToken RefreshToken(string token);
        public SecurityToken ValidateAccessToken(string accessToken, JwtSecurityTokenHandler tokenHandler);
        public Task<IdpResult> AddUserToIDP(MOrganizeRegistration orgUser);
    }
}
