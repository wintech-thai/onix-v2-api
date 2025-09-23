using Serilog;
using System.Security.Claims;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.Services;
using Its.Onix.Api.Utils;
using Microsoft.AspNetCore.Identity;

namespace Its.Onix.Api.Authentications
{
    public class BearerAuthenticationRepo : IBearerAuthenticationRepo
    {
        private readonly IOrganizationService? service = null;
        private readonly RedisHelper _redis;

        public BearerAuthenticationRepo(IOrganizationService svc, RedisHelper redis)
        {
            service = svc;
            _redis = redis;
        }

        private MVOrganizationUser? VerifyUser(string orgId, string user, HttpRequest request)
        {
            //This has not been tested
            //จะมี API บางตัวที่ไม่ต้องสนใจ user ว่าอยู่ใน org มั้ยเช่น UpdatePassword, GetAllAllowedOrg...

            var pc = ServiceUtils.GetPathComponent(request);
            var isWhiteListed = ServiceUtils.IsWhiteListedAPI(pc.ControllerName, pc.ApiName);

            if (isWhiteListed)
            {
                var ou = new MVOrganizationUser()
                {
                    Status = "OK",
                    Description = $"Whitelisted API [{pc.ControllerName}] [{pc.ApiName}]",

                    User = new Models.MUser() { UserName = user },
                    OrgUser = new Models.MOrganizationUser() { OrgCustomId = orgId },
                };
                //Console.WriteLine($"WHITELISTED ======= [{pc.ApiName}] [{pc.ControllerName}] ====");
                return ou;
            }

            var key = $"#{orgId}:VerifyUser:#{user}";
            var t = _redis.GetObjectAsync<MVOrganizationUser>(key);
            var orgUser = t.Result;

            if (orgUser == null)
            {
                var m = service!.VerifyUserInOrganization(orgId, user);
                _ = _redis.SetObjectAsync(key, m, TimeSpan.FromMinutes(5));

                orgUser = m;
            }

            return orgUser;
        }

        public User? Authenticate(string orgId, string user, string password, HttpRequest request)
        {
            var m = VerifyUser(orgId, user, request);
            if (m == null)
            {
                return null;
            }

            if (!m.Status!.Equals("OK"))
            {
                Log.Information(m.Description!);
                return null;
            }

            var u = new User()
            {
                UserName = user,
                Password = "",
                UserId = m.User!.UserId,
                Role = m.OrgUser!.RolesList,
                AuthenType = "JWT",
                OrgId = m.OrgUser.OrgCustomId,
                Email = m.User.UserEmail,
            };

            u.Claims = new[] {
                new Claim(ClaimTypes.NameIdentifier, u.UserId.ToString()!),
                new Claim(ClaimTypes.Name, user),
                new Claim(ClaimTypes.Role, u.Role!),
                //new Claim(ClaimTypes.Email, u.Email!),
                new Claim(ClaimTypes.AuthenticationMethod, u.AuthenType!),
                new Claim(ClaimTypes.Uri, request.Path),
                new Claim(ClaimTypes.GroupSid, u.OrgId!),
            };

            return u;
        }
    }
}
