using System.Security.Claims;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.Services;
using Its.Onix.Api.Utils;

namespace Its.Onix.Api.Authentications
{
    public class BasicAuthenticationRepo : IBasicAuthenticationRepo
    {
        private readonly IApiKeyService? service = null;
        private readonly IRedisHelper _redis;

        public BasicAuthenticationRepo(IApiKeyService svc, IRedisHelper redis)
        {
            service = svc;
            _redis = redis;
        }

        private MVApiKey? VerifyKey(string orgId, string password)
        {
            var key = $"#{orgId}:VerifyKey:#{password}";

            var t = _redis.GetObjectAsync<MVApiKey>(key);
            var mapiKey = t.Result;

            if (mapiKey == null)
            {
                //Not found
                //Console.WriteLine("################### GET FROM DB ##############3");
                var m = service!.VerifyApiKey(orgId, password);
                _ = _redis.SetObjectAsync(key, m, TimeSpan.FromMinutes(5));

                mapiKey = m;
            }

            return mapiKey;
        }

        public User? Authenticate(string orgId, string user, string password, HttpRequest request)
        {
            var m = VerifyKey(orgId, password);
            if (m == null)
            {
                return null;
            }

            var u = new User()
            {
                UserName = user,
                Password = m.ApiKey!.ApiKey,
                UserId = m.ApiKey.KeyId,
                Role = m.ApiKey.RolesList,
                AuthenType = "API-KEY",
                OrgId = m.ApiKey.OrgId,

                Status = m.Status,
                Description = m.Description,
            };

            u.Claims = [
                new Claim(ClaimTypes.NameIdentifier, u.UserId.ToString()!),
                new Claim(ClaimTypes.Name, user),
                new Claim(ClaimTypes.Role, u.Role!),
                new Claim(ClaimTypes.AuthenticationMethod, u.AuthenType!),
                new Claim(ClaimTypes.Uri, request.Path),
                new Claim(ClaimTypes.GroupSid, u.OrgId!),
            ];

            return u;
        }
    }
}
