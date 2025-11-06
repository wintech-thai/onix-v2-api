using Serilog;
using System.Security.Claims;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.Services;
using Its.Onix.Api.Utils;
using Its.Onix.Api.Models;

namespace Its.Onix.Api.Authentications
{
    public class BearerAuthenticationAdminRepo : IBearerAuthenticationAdminRepo
    {
        private readonly IOrganizationService? service = null;
        private readonly IRedisHelper _redis;

        public BearerAuthenticationAdminRepo(IOrganizationService svc, IRedisHelper redis)
        {
            service = svc;
            _redis = redis;
        }

        private MVOrganizationUser? VerifyUser(string orgId, string user, HttpRequest request)
        {
            var pc = ServiceUtils.GetPathComponent(request);
            var isWhiteListed = ServiceUtils.IsAdminWhiteListedAPI(pc.ControllerName, pc.ApiName);

            if (isWhiteListed)
            {
                var ou = new MVOrganizationUser()
                {
                    Status = "OK",
                    Description = $"Whitelisted API [{pc.ControllerName}] [{pc.ApiName}]",

                    User = new MUser() { UserName = user },
                    OrgUser = new MOrganizationUser() { OrgCustomId = orgId },
                };
                //Console.WriteLine($"WHITELISTED ======= [{pc.ApiName}] [{pc.ControllerName}] ====");
                return ou;
            }

            var key = $"#{orgId}:VerifyAdminUser:#{user}";
            var t = _redis.GetObjectAsync<MVOrganizationUser>(key);
            var orgUser = t.Result;

            if (orgUser == null)
            {
                //TODO : เปลี่ยนตรงนี้เป็นว่ามี userName อยู่ใน table Administrators หรือไม่
                var m = service!.VerifyUserInOrganization(orgId, user);
                _ = _redis.SetObjectAsync(key, m, TimeSpan.FromMinutes(5));

                orgUser = m;
            }

            if (orgUser != null)
            {
                // Check ตรงนี้หลังจากที่มี verify แล้วมี user อยู่ใน Organization
                // Check ต่อว่ามี Session อยู่ใน Redis ที่ setup ไว้ตอนที่ login หรือไม่
                // ต้องเช็คตรงนี้เพื่อทำเรื่องการ logout (เรียก API /logout) แบบทันที session ต้องหลุด

                var sessionKey = CacheHelper.CreateAdminLoginSessionKey(user);
                var session = _redis.GetObjectAsync<UserToken>(sessionKey);
                if (session.Result == null)
                {
                    var ou = new MVOrganizationUser()
                    {
                        Status = "ADMIN_SESSION_NOT_FOUND",
                        Description = $"Session not found please re-login for username [{user}]",

                        User = new MUser() { UserName = user },
                        OrgUser = new MOrganizationUser() { OrgCustomId = orgId },
                    };

                    return ou;
                }
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
