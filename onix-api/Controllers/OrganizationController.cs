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
        [HttpPost]
        [Route("org/{id}/action/UpdateOrganization")]
        public async Task<IActionResult> UpdateOrganization(string id, [FromBody] MOrganization request)
        {
            var result = await svc.UpdateOrganization(id, request);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{id}/action/GetLogoImageUploadPresignedUrl")]
        public IActionResult GetLogoImageUploadPresignedUrl(string id)
        {
            var result = svc.GetLogoImageUploadPresignedUrl(id);
            Response.Headers.Append("CUST_STATUS", result!.Status);

            return Ok(result);
        }
    }
}
