using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Models;
using Its.Onix.Api.Services;
using Its.Onix.Api.ViewsModels;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.Utils;
using System.Text.Json;
using System.Text;
using System.Web;
using YamlDotNet.Serialization.BufferedDeserialization.TypeDiscriminators;

namespace Its.Onix.Api.Controllers
{
    [ApiController]
    [Authorize(Policy = "GenericRolePolicy")]
    [Route("/admin-api/[controller]")]
    public class AdminOrganizationController : ControllerBase
    {
        private readonly IOrganizationService _orgSvc;
        private readonly IOrganizationUserService _orgUserSvc;
        private readonly IApiKeyService _apiKeySvc;
        private readonly IRedisHelper _redis;

        public AdminOrganizationController(IOrganizationService service,
            IOrganizationUserService orgUserSvc,
            IApiKeyService apiKeySvc,
            IRedisHelper redis)
        {
            _orgSvc = service;
            _orgUserSvc = orgUserSvc;
            _apiKeySvc = apiKeySvc;
            _redis = redis;
        }

        private string CreateForgotPasswordLink(string orgId, MUserRegister reg)
        {
            var regType = "forgot-password";

            var jsonString = JsonSerializer.Serialize(reg);
            byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonString);
            string jsonStringB64 = Convert.ToBase64String(jsonBytes);

            var dataUrlSafe = HttpUtility.UrlEncode(jsonStringB64);

            var registerDomain = "<REGISTER_SERVICE_DOMAIN>"; //คนที่เรียกใช้งานจะต้องเปลี่ยนเป็น domain ของ register service เอง

            var token = Guid.NewGuid().ToString();
            var registrationUrl = $"https://{registerDomain}/{regType}/{orgId}/{token}?data={dataUrlSafe}";

            //ใส่ data ไปที่ Redis เพื่อให้ register service มาดึงข้อมูลไปใช้ต่อ
            var cacheKey = CacheHelper.CreateApiOtpKey(orgId, "UserForgotPassword");
            _ = _redis.SetObjectAsync($"{cacheKey}:{token}", reg, TimeSpan.FromMinutes(60 * 24)); //หมดอายุ 1 วัน

            return registrationUrl;
        }

        [HttpGet]
        [Route("org/global/action/GetOrgUserForgotPasswordLink/{orgId}/{orgUserId}")]
        public IActionResult GetOrgUserForgotPasswordLink(string orgId, string orgUserId)
        {
            var mv = new MVOrganizationUserRegistration()
            {
                Status = "OK",
                Description = "Success"
            };

            var svcStatus = _orgUserSvc.GetUserByIdLeftJoin(orgId, orgUserId);
            if (svcStatus.Status != "OK")
            {
                return Ok(svcStatus);
            }

            var user = svcStatus.OrgUser!;
            if (user == null)
            {
                mv.Status = "EMPTY_USER_RETURN";
                mv.Description = $"No user return for org user ID [{orgUserId}] !!!";

                return Ok(mv);
            }

            if (user.UserStatus != "Active")
            {
                mv.Status = "USER_NOT_ACTIVE";
                mv.Description = $"User status is [{user.UserStatus}] for org user ID [{orgUserId}] !!!";

                return Ok(mv);
            }

            var reg = new MUserRegister()
            {
                Email = user.UserEmail,
                UserName = user.UserName!,
                OrgUserId = orgId,
            };

            var forgotPasswordUrl = CreateForgotPasswordLink(orgId, reg);
            mv.ForgotPasswordUrl = forgotPasswordUrl;

            return Ok(mv);
        }

        [HttpPost]
        [Route("org/global/action/AddOrganization")]
        public IActionResult AddOrganization([FromBody] MOrganization request)
        {
            var result = _orgSvc.AddOrganization("notused", request);
            return Ok(result);
        }


        [HttpPost]
        [Route("org/global/action/CreatePaymentEndpointsApiKey/{orgId}/{roles}")]
        public IActionResult CreatePaymentEndpointsApiKey(string orgId, string roles)
        {
            var uuid = Guid.NewGuid();

            var request = new MApiKey()
            {
                KeyType = "Payment",
                KeyName = $"Payment:{uuid}",
                KeyDescription = "Auto generated key for Payment API, DO NOT delete!!!",
                Roles = [.. roles.Split(',')], //แกะ roles ที่คั่นด้วย comma ออกมาเป็น array
            };

            var apiKey = _apiKeySvc.AddApiKey(orgId, request);
            return Ok(apiKey);
        }

        [HttpPost]
        [Route("org/global/action/CreatePaymentRequestApiKey/{orgId}")]
        public IActionResult CreatePaymentRequestApiKey(string orgId)
        {
            var uuid = Guid.NewGuid();

            var request = new MApiKey()
            {
                KeyType = "PaymentRequest",
                KeyName = $"PayInRequest:{uuid}",
                KeyDescription = "Auto generated key, DO NOT delete!!!",
                Roles = [ "PAYMENT_REQUEST", "PAYOUT_REQUEST" ], //เป็น system role สำหรับ API SubmitPaymentRequest() โดยเฉพาะ
            };

            var apiKey = _apiKeySvc.AddApiKey(orgId, request);
            return Ok(apiKey);
        }


        [HttpPost]
        [Route("org/global/action/CreatePayOutRequestApiKey/{orgId}")]
        public IActionResult CreatePayOutRequestApiKey(string orgId)
        {
            var uuid = Guid.NewGuid();

            var request = new MApiKey()
            {
                KeyType = "PayOut",
                KeyName = $"PayOutRequest:{uuid}",
                KeyDescription = "Auto generated key, DO NOT delete!!!",
                Roles = [ "PAYOUT_REQUEST", "PAYMENT_REQUEST" ], //เป็น system role สำหรับ API SubmitPaymentRequest() โดยเฉพาะ
            };

            var apiKey = _apiKeySvc.AddApiKey(orgId, request);
            return Ok(apiKey);
        }

        [HttpGet]
        [Route("org/global/action/GetPaymentRequestApiKeys/{orgId}")]
        public IActionResult GetPaymentRequestApiKeys(string orgId)
        {
            var request = new VMApiKey()
            {
                KeyType = "PaymentRequest", 
            };

            var keys = _apiKeySvc.GetApiKeys(orgId, request);

            return Ok(keys);
        }

        [HttpGet]
        [Route("org/global/action/GetPayOutRequestApiKeys/{orgId}")]
        public IActionResult GetPayOutRequestApiKeys(string orgId)
        {
            var request = new VMApiKey()
            {
                KeyType = "PayOut", 
            };

            var keys = _apiKeySvc.GetApiKeys(orgId, request);

            return Ok(keys);
        }

        [HttpGet]
        [Route("org/global/action/GetPaymentApiKeys/{orgId}")]
        public IActionResult GetPaymentApiKeys(string orgId)
        {
            var request = new VMApiKey()
            {
                KeyTypeSet = "Payment,PaymentRequest",
            };

            var keys = _apiKeySvc.GetApiKeys(orgId, request);

            return Ok(keys);
        }

        [HttpPost]
        [Route("org/global/action/DeletePaymentRequestApiKeyById/{orgId}/{apiKeyId}")]
        public IActionResult DeletePaymentRequestApiKeyById(string orgId, string apiKeyId)
        {
            var apiKey = _apiKeySvc.DeleteApiKeyById(orgId, apiKeyId);
            return Ok(apiKey);
        }

        [HttpPost]
        [Route("org/global/action/UpdatePaymentRequestApiKeyById/{orgId}/{apiKeyId}")]
        public IActionResult UpdatePaymentRequestApiKeyById(string orgId, string apiKeyId, [FromBody] MApiKey request)
        {
            var apiKey = _apiKeySvc.UpdateApiKeyById(orgId, apiKeyId, request);
            return Ok(apiKey);
        }

        [HttpPost]
        [Route("org/global/action/EnablePaymentRequestApiKeyById/{orgId}/{apiKeyId}")]
        public IActionResult EnablePaymentRequestApiKeyById(string orgId, string apiKeyId)
        {
            var apiKey = _apiKeySvc.UpdateApiKeyStatusById(orgId, apiKeyId, "Active");
            return Ok(apiKey);
        }

        [HttpPost]
        [Route("org/global/action/DisableRequestApiKeyById/{orgId}/{apiKeyId}")]
        public IActionResult DisableRequestApiKeyById(string orgId, string apiKeyId)
        {
            var apiKey = _apiKeySvc.UpdateApiKeyStatusById(orgId, apiKeyId, "Disabled");
            return Ok(apiKey);
        }

        [HttpPost]
        [Route("org/global/action/RegisterOrganization")]
        public IActionResult RegisterOrganization([FromBody] MOrganizeRegistration request)
        {
            var invitedBy = "unknown";

            var nameObj = Response.HttpContext.Items["Temp-Identity-Name"];
            if (nameObj != null)
            {
                invitedBy = nameObj.ToString();
            }

            var orgId = request.UserOrgId!;
            var org = new MOrganization()
            {
                OrgCustomId = orgId,
                OrgName = request.Name,
                OrgType = request.UserOrgType,
            };

            var orgStatus = _orgSvc.AddOrganization(orgId, org);
            if (orgStatus.Status != "OK")
            {
                return Ok(orgStatus);
            }

            var ou = new MOrganizationUser()
            {
                UserName = request.UserName,
                TmpUserEmail = request.Email,
                InvitedBy = invitedBy,
                IsOrgInitialUser = "YES",
                Roles = [ "OWNER" ],
            };
            var orgUserStatus = _orgUserSvc.InviteUser(orgId, ou);

            return Ok(orgUserStatus);
        }

        //#### OrgUsers
        [HttpPost]
        [Route("org/global/action/InviteOrganizationUser/{orgId}")]
        public IActionResult InviteOrganizationUser(string orgId, [FromBody] MOrganizationUser request)
        {
            var invitedBy = "unknown";

            var nameObj = Response.HttpContext.Items["Temp-Identity-Name"];
            if (nameObj != null)
            {
                invitedBy = nameObj.ToString();
            }

            var ou = new MOrganizationUser()
            {
                UserName = request.UserName,
                TmpUserEmail = request.UserEmail,
                InvitedBy = invitedBy,
                InvitedByAdmin = true,
                Roles = [ "OWNER" ],
            };
            var orgUserStatus = _orgUserSvc.InviteUserWithLink(orgId, ou);

            return Ok(orgUserStatus);
        }

        [HttpPost]
        [Route("org/global/action/EnableOrgUserById/{orgId}/{orgUserId}")]
        public IActionResult EnableOrgUserById(string orgId, string orgUserId)
        {
            var apiKey = _orgUserSvc.UpdateUserStatusById(orgId, orgUserId, "Active");
            return Ok(apiKey);
        }

        [HttpPost]
        [Route("org/global/action/DisableOrgUserById/{orgId}/{orgUserId}")]
        public IActionResult DisableOrgUserById(string orgId, string orgUserId)
        {
            var apiKey = _orgUserSvc.UpdateUserStatusById(orgId, orgUserId, "Disabled");
            return Ok(apiKey);
        }

        [HttpDelete]
        [Route("org/global/action/DeleteOrgUserById/{orgId}/{orgUserId}")]
        public IActionResult DeleteOrgUserById(string orgId, string orgUserId)
        {
            var mvUser = _orgUserSvc.GetUserByIdLeftJoin(orgId, orgUserId);
            if (mvUser.Status != "OK")
            {
                return Ok(mvUser);
            }

            var user = mvUser.OrgUser!;
            if (user.UserStatus != "Pending")
            {
                mvUser.Status = "ERROR_ONLY_ALLOW_FOR_PENDING_USER";
                mvUser.Description = "Only allow for delete peinding user!!!";
                return Ok(mvUser);
            }

            var result = _orgUserSvc.DeleteUserById(orgId, orgUserId);
            return Ok(result);
        }

        [HttpGet]
        [Route("org/global/action/GetOrgUsers/{orgId}")]
        public IActionResult GetOrgUsers(string orgId)
        {
            var request = new VMOrganizationUser()
            {
                FullTextSearch = "", 
            };

            var users = _orgUserSvc.GetUsersLeftJoin(orgId, request);

            return Ok(users);
        }
    }
}
