using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Services;

namespace Its.Onix.Api.Controllers
{
    [ApiController]
    [Authorize(Policy = "GenericRolePolicy")]
    [Route("/api/[controller]")]
    public class AuditNoticeController : ControllerBase
    {
        private readonly IAuditNoticeService svc;

        [ExcludeFromCodeCoverage]
        public AuditNoticeController(IAuditNoticeService service)
        {
            svc = service;
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{orgId}/action/GetAuditNoticesByRowId/{rowId}")]
        public async Task<IActionResult> GetAuditNoticesByRowId(string orgId, string rowId)
        {
            var result = await svc.GetByOrgIdAndRowId(orgId, rowId);
            return Ok(result);
        }
    }
}
