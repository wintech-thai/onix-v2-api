using Microsoft.IdentityModel.Tokens;

namespace Its.Onix.Api.Authentications
{
    public interface IJwtSigner
    {
        public SecurityKey GetSignedKey(string? url);
    }
}
