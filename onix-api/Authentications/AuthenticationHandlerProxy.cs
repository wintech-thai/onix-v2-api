using System.Text;
using System.Text.Encodings.Web;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Its.Onix.Api.Services;

namespace Its.Onix.Api.Authentications
{
    public class AuthenticationHandlerProxy : AuthenticationHandlerProxyBase
    {
        private readonly IBasicAuthenticationRepo? basicAuthenRepo = null;
        private readonly IBearerAuthenticationRepo? bearerAuthRepo = null;
        private readonly IConfiguration config;
        private readonly IAuthService _authService;
        private JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();

        [Obsolete]
        public AuthenticationHandlerProxy(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            IBasicAuthenticationRepo bsAuthRepo,
            IBearerAuthenticationRepo brAuthRepo,
            IConfiguration cfg,
            IAuthService authService,
            ISystemClock clock) : base(options, logger, encoder, clock)
        {
            basicAuthenRepo = bsAuthRepo;
            bearerAuthRepo = brAuthRepo;
            config = cfg;
            _authService = authService;
        }

        protected override User? AuthenticateBasic(string orgId, byte[]? jwtBytes, HttpRequest request)
        {
            var credentials = Encoding.UTF8.GetString(jwtBytes!).Split(new[] { ':' }, 2);
            var username = credentials[0];
            var password = credentials[1];

            var user = basicAuthenRepo!.Authenticate(orgId, username, password, request);
            return user;
        }

        protected override User? AuthenticateBearer(string orgId, byte[]? jwtBytes, HttpRequest request)
        {
            var accessToken = Encoding.UTF8.GetString(jwtBytes!);

            //Throw exception if invalid
            _authService.ValidateAccessToken(accessToken, tokenHandler);

            var jwt = tokenHandler.ReadJwtToken(accessToken);
            string userName = jwt.Claims.First(c => c.Type == "preferred_username").Value;

            var user = bearerAuthRepo!.Authenticate(orgId, userName, "", request);

            return user;
        }
    }
}