using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Services;

namespace Its.Onix.Api.Controllers
{
    //x088 : ให้ merchant ดู audit trail ได้เหมือนกับที่ admin ดูได้ แต่จำกัดแค่ของ org ตัวเองเท่านั้น
    [ApiController]
    [Authorize(Policy = "GenericRolePolicy")]
    [Route("/api/[controller]")]
    public class AuditTrailController : ControllerBase
    {
        private readonly IAuditTrailService svc;

        [ExcludeFromCodeCoverage]
        public AuditTrailController(IAuditTrailService service)
        {
            svc = service;
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{orgId}/action/GetAuditTrailByRowId/{rowId}")]
        public async Task<IActionResult> GetAuditTrailByRowId(string orgId, string rowId)
        {
            var result = await svc.GetByRowId(orgId, rowId);
            return Ok(result);
        }
    }
}
