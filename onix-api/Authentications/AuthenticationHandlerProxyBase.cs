using Serilog;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Its.Onix.Api.Utils;

namespace Its.Onix.Api.Authentications
{
    public abstract class AuthenticationHandlerProxyBase : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        protected abstract AuthenResult? AuthenticateBasic(string orgId, byte[]? jwtBytes, HttpRequest request);
        protected abstract AuthenResult? AuthenticateBearer(string orgId, byte[]? jwtBytes, HttpRequest request);

        [Obsolete]
        protected AuthenticationHandlerProxyBase(
            IOptionsMonitor<AuthenticationSchemeOptions> options, 
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock) : base(options, logger, encoder, clock)
        {
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue("Authorization", out var authData))
            {
                var msg = "No Authorization header found";
                await Response.WriteAsync(msg);

                return AuthenticateResult.Fail(msg);
            }

            var authHeader = AuthenticationHeaderValue.Parse(authData!);
            if (!authHeader.Scheme.Equals("Bearer") && !authHeader.Scheme.Equals("Basic"))
            {
                var msg = $"Unknown scheme [{authHeader.Scheme}]";
                await Response.WriteAsync(msg);

                return AuthenticateResult.Fail(msg);
            }

            var authResult = new AuthenResult();
            try
            {
                var orgId = ServiceUtils.GetOrgId(Request);
                var credentialBytes = Convert.FromBase64String(authHeader.Parameter!);

                if (authHeader.Scheme.Equals("Basic"))
                {
                    authResult = await Task.Run(() => AuthenticateBasic(orgId, credentialBytes, Request));
                }
                else
                {
                    //Bearer
                    authResult = await Task.Run(() => AuthenticateBearer(orgId, credentialBytes, Request));
                }
            }
            catch (Exception e)
            {
                var msg = e.Message;
                await Response.WriteAsync(msg);

                Log.Error($"[AuthenticationHandlerProxyBase] --> [{msg}]");
                return AuthenticateResult.Fail($"Invalid Authorization Header for [{authHeader.Scheme}]");
            }

            if (authResult!.UserAuthen == null)
            {
                var msg = $"User not found [{authResult.UserName}], scheme=[{authHeader.Scheme}]";
                await Response.WriteAsync(msg);

                return AuthenticateResult.Fail(msg);
            }

            var identity = new ClaimsIdentity(authResult.UserAuthen.Claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            Context.Request.Headers.Append("AuthenScheme", Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }
    }
}