using Serilog;
using System.Security.Claims;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.Services;
using Its.Onix.Api.Utils;
using Its.Onix.Api.Models;

namespace Its.Onix.Api.Authentications
{
    public class BearerAuthenticationCustomerRepo : IBearerAuthenticationCustomerRepo
    {
        private readonly IEntityService? service = null;
        private readonly IRedisHelper _redis;

        public BearerAuthenticationCustomerRepo(IEntityService svc, IRedisHelper redis)
        {
            service = svc;
            _redis = redis;
        }

        private MVCustomerUser? VerifyUser(string orgId, string user, HttpRequest request)
        {
            var pc = ServiceUtils.GetPathComponent(request);
            var isWhiteListed = ServiceUtils.IsCustomerWhiteListedAPI(pc.ControllerName, pc.ApiName);

            if (isWhiteListed)
            {
                var ou = new MVCustomerUser()
                {
                    Status = "OK",
                    Description = $"Whitelisted API [{pc.ControllerName}] [{pc.ApiName}]",

                    User = new MUser() { UserName = user },
                    CustomerUser = new MEntity() { UserName = user },
                };
                //Console.WriteLine($"WHITELISTED ======= [{pc.ApiName}] [{pc.ControllerName}] ====");
                return ou;
            }

            var key = $"#{orgId}:VerifyCustomerUser:#{user}";
            var t = _redis.GetObjectAsync<MVCustomerUser>(key);
            var customerUser = t.Result;

            if (customerUser == null)
            {
                var m = service!.VerifyUserIsCustomer(user);
                _ = _redis.SetObjectAsync(key, m, TimeSpan.FromMinutes(5));

                customerUser = m;
            }

            if (customerUser != null)
            {
                // Check ตรงนี้หลังจากที่มี verify แล้วมี user อยู่ใน Organization
                // Check ต่อว่ามี Session อยู่ใน Redis ที่ setup ไว้ตอนที่ login หรือไม่
                // ต้องเช็คตรงนี้เพื่อทำเรื่องการ logout (เรียก API /logout) แบบทันที session ต้องหลุด

                var sessionKey = CacheHelper.CreateCustomerLoginSessionKey(user);
                var session = _redis.GetObjectAsync<UserToken>(sessionKey);
                if (session.Result == null)
                {
                    var ou = new MVCustomerUser()
                    {
                        Status = "CUSTOMER_SESSION_NOT_FOUND",
                        Description = $"Session not found please re-login for username [{user}]",

                        User = new MUser() { UserName = user },
                        CustomerUser = new MEntity() { UserName = user },
                    };

                    return ou;
                }
            }

            return customerUser;
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
Console.WriteLine($"DEBUG_100 : [{m.Status}] [{orgId}], email=[{m.User!.UserEmail}], uname=[{user}], uid=[{m.User!.UserId}]");
            var u = new User()
            {
                UserName = user, //customer:<org-id>:<entity-id>
                Password = "",
                UserId = m.User!.UserId,
                Role = "CUSTOMER", //มี role เดียวจัดการทุกอย่างได้เฉพาะของตัวเองเท่านั้น
                AuthenType = "JWT",
                OrgId = orgId,
                Email = m.User.UserEmail,

                Status = m.Status,
                Description = m.Description,
            };
Console.WriteLine($"DEBUG_101 : [{m.Status}] [{orgId}], email=[{m.User!.UserEmail}], uname=[{user}], uid=[{m.User!.UserId}]");
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
