using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Services;

namespace Its.Onix.Api.Controllers
{
    [Authorize(Policy = "GenericRolePolicy")]
    [ApiController]
    [Route("/admin-api/[controller]")]
    public class AdminAuditTrailController : ControllerBase
    {
        private readonly IAuditTrailService svc;

        public AdminAuditTrailController(IAuditTrailService service)
        {
            svc = service;
        }

        [HttpGet]
        [Route("org/global/action/GetAuditTrailByRowId/{rowId}")]
        public async Task<IActionResult> GetAuditTrailByRowId(string rowId)
        {
            var result = await svc.GetByRowId(rowId);
            return Ok(result);
        }
    }
}
