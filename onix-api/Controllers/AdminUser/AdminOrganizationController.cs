using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Models;
using Its.Onix.Api.Services;

namespace Prom.LPR.Api.Controllers
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
        [Route("org/global/action/RegisterOrganization")]
        public IActionResult RegisterOrganization([FromBody] MOrganizeRegistration request)
        {
            var orgId = request.UserOrgId!;
            var org = new MOrganization()
            {
                OrgCustomId = orgId,
                OrgName = request.Name,
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
                IsOrgInitialUser = "YES",
                Roles = [ "OWNER" ],
            };
            var orgUserStatus = _orgUserSvc.InviteUser(orgId, ou);

            return Ok(orgUserStatus);
        }
    }
}
