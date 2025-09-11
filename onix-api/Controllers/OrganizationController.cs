using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Models;
using Its.Onix.Api.Services;

namespace Prom.LPR.Api.Controllers
{
    [ApiController]
    [Authorize(Policy = "GenericRolePolicy")]
    [Route("/api/[controller]")]
    public class OrganizationController : ControllerBase
    {
        private readonly IOrganizationService svc;

        [ExcludeFromCodeCoverage]
        public OrganizationController(IOrganizationService service)
        {
            svc = service;
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{id}/action/GetOrganization")]
        public async Task<IActionResult> GetOrganization(string id)
        {
            var result = await svc.GetOrganization(id);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{id}/action/GetUserAllowedOrg")]
        public IActionResult GetUserAllowedOrg()
        {
            var idTypeObj = Response.HttpContext.Items["Temp-Identity-Type"];
            if (idTypeObj == null)
            {
                return BadRequest("Unable to identify identity type!!!");
            }

            var idType = idTypeObj.ToString();
            if (idType != "JWT")
            {
                return BadRequest("Only allow for JWT identity type!!!");
            }

            var nameObj = Response.HttpContext.Items["Temp-Identity-Name"];
            if (nameObj == null)
            {
                return BadRequest("Unable to find user name!!!");
            }

            var userName = nameObj.ToString();
            if (userName == "")
            {
                return BadRequest("User name is empty!!!");
            }

            var result = svc.GetUserAllowedOrganization(userName!);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/AddUserToOrganization")]
        public IActionResult AddUserToOrganization(string id, [FromBody] MOrganizationUser request)
        {
            var result = svc.AddUserToOrganization(id, request);
            if (result!.Status != "OK")
            {
                return BadRequest(result!.Description);
            }
            
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/AdminAddOrganization")]
        public IActionResult AdminAddOrganization(string id, [FromBody] MOrganization request)
        {
            // 'id' must be 'global' to use Admin* API
            var result = svc.AddOrganization(id, request);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/AdminAddUserToOrganization")]
        public IActionResult AdminAddUserToOrganization(string id, [FromBody] MOrganizationUser request)
        {
            var userOrgId = request.OrgCustomId;
            var result = svc.AddUserToOrganization(userOrgId!, request);
            return Ok(result);
        }
    }
}
