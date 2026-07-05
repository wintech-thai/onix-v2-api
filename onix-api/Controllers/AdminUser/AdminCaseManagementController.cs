using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Services;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Controllers
{
    [ApiController]
    [Authorize(Policy = "GenericRolePolicy")]
    [Route("/admin-api/[controller]")]
    public class AdminCaseManagementController : ControllerBase
    {
        private readonly ICaseManagementService svc;

        [ExcludeFromCodeCoverage]
        public AdminCaseManagementController(ICaseManagementService service)
        {
            svc = service;
        }

        private string GetUsername() =>
            User.FindFirst("Temp-Identity-name")?.Value
            ?? User.FindFirst(ClaimTypes.Name)?.Value
            ?? "admin";

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/GetCases")]
        public async Task<IActionResult> GetCases([FromBody] VMCaseManagement param)
        {
            var result = await svc.GetAllCases(param);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/GetCaseCount")]
        public async Task<IActionResult> GetCaseCount([FromBody] VMCaseManagement param)
        {
            var result = await svc.GetAllCaseCount(param);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/global/action/GetCaseById/{caseId}")]
        public async Task<IActionResult> GetCaseById(string caseId)
        {
            var result = await svc.GetCaseById("global", caseId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/UpdateCaseStatus/{caseId}/{status}")]
        public async Task<IActionResult> UpdateCaseStatus(string caseId, string status)
        {
            var result = await svc.UpdateCaseStatus("global", caseId, status, GetUsername());
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/AddComment/{caseId}")]
        public async Task<IActionResult> AddComment(string caseId, [FromBody] VMCaseManagementComment request)
        {
            var result = await svc.AddComment("global", caseId, request.Content ?? "", GetUsername(), "Admin");
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/global/action/GetComments/{caseId}")]
        public async Task<IActionResult> GetComments(string caseId)
        {
            var result = await svc.GetComments("global", caseId);
            return Ok(result);
        }
    }
}
