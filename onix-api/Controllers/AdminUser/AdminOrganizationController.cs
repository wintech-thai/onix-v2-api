using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Models;
using Its.Onix.Api.Services;

namespace Its.Onix.Api.Controllers
{
    [ApiController]
    [Authorize(Policy = "GenericRolePolicy")]
    [Route("/admin-api/[controller]")]
    public class AdminOrganizationController : ControllerBase
    {
        private readonly IOrganizationService _orgSvc;
        private readonly IOrganizationUserService _orgUserSvc;

        public AdminOrganizationController(IOrganizationService service, IOrganizationUserService orgUserSvc)
        {
            _orgSvc = service;
            _orgUserSvc = orgUserSvc;
        }

        [HttpPost]
        [Route("org/global/action/AddOrganization")]
        public IActionResult AddOrganization([FromBody] MOrganization request)
        {
            var result = _orgSvc.AddOrganization("notused", request);
            return Ok(result);
        }

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
    }
}
