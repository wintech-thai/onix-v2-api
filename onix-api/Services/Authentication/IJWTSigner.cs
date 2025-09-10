using Microsoft.IdentityModel.Tokens;

namespace Its.Onix.Api.Services
{
    public interface IJwtSigner
    {
        public SecurityKey GetSignedKey(string? url);
    }
}
