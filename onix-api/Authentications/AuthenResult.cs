using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;

namespace Its.Onix.Api.Authentications
{
    [ExcludeFromCodeCoverage]
    public class AuthenResult
    {
        public User? UserAuthen {get; set;}
        public string? UserName {get; set;}

        public AuthenResult()
        {
        }
    }
}
