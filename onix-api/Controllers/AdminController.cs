using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Models;
using Its.Onix.Api.Services;
using Its.Onix.Api.ModelsViews;

namespace Prom.LPR.Api.Controllers
{
    [ApiController]
    [Authorize(Policy = "GenericRolePolicy")]
    [Route("/api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService svc;

        [ExcludeFromCodeCoverage]
        public AdminController(IAdminService service)
        {
            svc = service;
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/RegisterOrganization")]
        public IActionResult RegisterOrganization(string id, [FromBody] MOrganizeRegistration request)
        {
            var result = svc.RegisterOrganization(id, request);

            Response.Headers.Append("CUST_STATUS", result.Status);
            Response.Headers.Append("CUST_DESC", result.Description);

            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{id}/action/SendOrgRegisterOtpEmail/{email}")]
        public IActionResult SendOtpViaEmail(string id, string email)
        {
            var result = svc.SendOrgRegisterOtpEmail(id, email);

            Response.Headers.Append("CUST_STATUS", result.Status);
            Response.Headers.Append("CUST_DESC", result.Description);
            
            return Ok(result);
        }
    }
}
