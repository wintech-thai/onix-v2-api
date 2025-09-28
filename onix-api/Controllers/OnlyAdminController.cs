using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Models;
using Its.Onix.Api.Services;

namespace Prom.LPR.Api.Controllers
{
    [ApiController]
    [Authorize(Policy = "GenericRolePolicy")]
    [Route("/api/[controller]")]
    public class OnlyAdminController : ControllerBase
    {
        private readonly IAdminService svc;

        public OnlyAdminController(IAdminService service)
        {
            svc = service;
        }

        [HttpPost]
        [Route("org/global/action/RegisterOrganization")]
        public IActionResult RegisterOrganization([FromBody] MOrganizeRegistration request)
        {
            var result = svc.RegisterOrganization("global", request);

            Response.Headers.Append("CUST_STATUS", result.Status);
            Response.Headers.Append("CUST_DESC", result.Description);

            return Ok(result);
        }

        [HttpGet]
        [Route("org/global/action/SendOrgRegisterOtpEmail/{email}")]
        public IActionResult SendOtpViaEmail(string email)
        {
            var result = svc.SendOrgRegisterOtpEmail("global", email);

            Response.Headers.Append("CUST_STATUS", result.Status);
            Response.Headers.Append("CUST_DESC", result.Description);

            return Ok(result);
        }

        [HttpGet]
        [Route("org/global/action/IsOrgIdExist/{orgid}")]
        public IActionResult IsOrgIdExist(string orgid)
        {
            var result = svc.IsOrganizationExist(orgid);
            return Ok(result);
        }

        [HttpGet]
        [Route("org/global/action/IsUserNameExist/{username}")]
        public IActionResult IsUserNameExist(string userName)
        {
            var result = svc.IsUserNameExist(userName);
            return Ok(result);
        }
        
        [HttpGet]
        [Route("org/global/action/IsEmailExist/{email}")]
        public IActionResult IsEmailExist(string email)
        {
            var result = svc.IsEmailExist(email);
            return Ok(result);
        }
    }
}
