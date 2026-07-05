using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Models;
using Its.Onix.Api.Services;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Controllers
{
    [ApiController]
    [Authorize(Policy = "GenericRolePolicy")]
    [Route("/api/[controller]")]
    public class CaseManagementController : ControllerBase
    {
        private readonly ICaseManagementService svc;

        [ExcludeFromCodeCoverage]
        public CaseManagementController(ICaseManagementService service)
        {
            svc = service;
        }

        private string GetUsername() =>
            User.FindFirst("Temp-Identity-name")?.Value
            ?? User.FindFirst(ClaimTypes.Name)?.Value
            ?? "unknown";

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{orgId}/action/GetCases")]
        public async Task<IActionResult> GetCases(string orgId, [FromBody] VMCaseManagement param)
        {
            var result = await svc.GetCases(orgId, param);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{orgId}/action/GetCaseCount")]
        public async Task<IActionResult> GetCaseCount(string orgId, [FromBody] VMCaseManagement param)
        {
            var result = await svc.GetCaseCount(orgId, param);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{orgId}/action/GetCaseById/{caseId}")]
        public async Task<IActionResult> GetCaseById(string orgId, string caseId)
        {
            var result = await svc.GetCaseById(orgId, caseId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{orgId}/action/AddCase")]
        public async Task<IActionResult> AddCase(string orgId, [FromBody] MCaseManagement request)
        {
            request.CreatedBy = GetUsername();
            var result = await svc.AddCase(orgId, request);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{orgId}/action/UpdateCaseStatus/{caseId}/{status}")]
        public async Task<IActionResult> UpdateCaseStatus(string orgId, string caseId, string status)
        {
            var result = await svc.UpdateCaseStatus(orgId, caseId, status, GetUsername());
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{orgId}/action/AddComment/{caseId}")]
        public async Task<IActionResult> AddComment(string orgId, string caseId, [FromBody] VMCaseManagementComment request)
        {
            var result = await svc.AddComment(orgId, caseId, request.Content ?? "", GetUsername(), "Merchant");
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{orgId}/action/GetComments/{caseId}")]
        public async Task<IActionResult> GetComments(string orgId, string caseId)
        {
            var result = await svc.GetComments(orgId, caseId);
            return Ok(result);
        }
    }
}
