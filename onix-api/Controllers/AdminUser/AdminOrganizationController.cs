using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Models;
using Its.Onix.Api.Services;
using Its.Onix.Api.ViewsModels;using YamlDotNet.Serialization.BufferedDeserialization.TypeDiscriminators;

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

        public AdminOrganizationController(IOrganizationService service, 
            IOrganizationUserService orgUserSvc,
            IApiKeyService apiKeySvc)
        {
            _orgSvc = service;
            _orgUserSvc = orgUserSvc;
            _apiKeySvc = apiKeySvc;
        }

        [HttpPost]
        [Route("org/global/action/AddOrganization")]
        public IActionResult AddOrganization([FromBody] MOrganization request)
        {
            var result = _orgSvc.AddOrganization("notused", request);
            return Ok(result);
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
                Roles = [ "PAYMENT_REQUEST" ], //เป็น system role สำหรับ API SubmitPaymentRequest() โดยเฉพาะ
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
                Roles = [ "PAYOUT_REQUEST" ], //เป็น system role สำหรับ API SubmitPaymentRequest() โดยเฉพาะ
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

        [HttpPost]
        [Route("org/global/action/DeletePaymentRequestApiKeyById/{orgId}/{apiKeyId}")]
        public IActionResult DeletePaymentRequestApiKeyById(string orgId, string apiKeyId)
        {
            var apiKey = _apiKeySvc.DeleteApiKeyById(orgId, apiKeyId);
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
