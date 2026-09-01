using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Models;
using Its.Onix.Api.Services;
using Its.Onix.Api.Utils;
using System.Security.Claims;

namespace Prom.LPR.Api.Controllers
{
    [ApiController]
    [Authorize(Policy = "GenericRolePolicy")]
    [Route("/api/[controller]")]
    public class OrganizationController : ControllerBase
    {
        private readonly IOrganizationService svc;
        private readonly IConfigurationService configSvc;

        [ExcludeFromCodeCoverage]
        public OrganizationController(IOrganizationService service, IConfigurationService configService)
        {
            svc = service;
            configSvc = configService;
        }

        private string? GetCurrentOrgId()
        {
            return User.FindFirst(ClaimTypes.GroupSid)?.Value;
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{id}/action/company-profile")]
        public async Task<IActionResult> GetCompanyProfile(string id)
        {
            var result = await svc.GetCompanyProfile(id);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/company-profile/update")]
        public async Task<IActionResult> UpdateCompanyProfile(string id, [FromBody] MOrganization request)
        {
            var result = await svc.UpdateCompanyProfile(id, request);
            return Ok(result);
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

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{id}/action/GetAllowChannelNames")]
        public IActionResult GetAllowChannelNames(string id)
        {
            var result = svc.GetAllowChannelNames(id);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{id}/action/GetAllowAddressTypeNames")]
        public IActionResult GetAllowAddressTypeNames(string id)
        {
            var result = svc.GetAllowAddressTypeNames(id);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{id}/action/GetOrganizationPolicy")]
        public async Task<IActionResult> GetOrganizationPolicy(string id)
        {
            var result = await svc.GetOrganizationPolicy(id);
            return Ok(result);
        }

        // Public: merchant web frontend calls this (before/without needing GenericRolePolicy)
        // to check whether the current client IP is blacklisted for this org's web front-end.
        [ExcludeFromCodeCoverage]
        [AllowAnonymous]
        [HttpGet]
        [Route("org/{id}/action/GetIpPolicyStatus")]
        public async Task<IActionResult> GetIpPolicyStatus(string id)
        {
            var clientIp = await ServiceUtils.ResolveConfiguredClientIp(Request, configSvc);
            var result = await svc.CheckIpBlacklist(id, clientIp, isApi: false);
            return Ok(result);
        }
    }
}
