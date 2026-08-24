using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Services;

namespace Its.Onix.Api.Controllers
{
    [Authorize(Policy = "GenericRolePolicy")]
    [ApiController]
    [Route("/admin-api/[controller]")]
    public class AdminAuditNoticeController : ControllerBase
    {
        private readonly IAuditNoticeService svc;

        public AdminAuditNoticeController(IAuditNoticeService service)
        {
            svc = service;
        }

        [HttpGet]
        [Route("org/global/action/GetAuditNoticesByRowId/{rowId}")]
        public async Task<IActionResult> GetAuditNoticesByRowId(string rowId)
        {
            var result = await svc.GetByRowId(rowId);
            return Ok(result);
        }
    }
}
