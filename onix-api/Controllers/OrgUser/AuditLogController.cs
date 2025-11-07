using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Services;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Controllers
{
    [Authorize(Policy = "GenericRolePolicy")]
    [ApiController]
    [Route("/api/[controller]")]
    public class AuditLogController : ControllerBase
    {
        private readonly IAuditLogService svc;

        public AuditLogController(IAuditLogService service)
        {
            svc = service;
        }

        [HttpGet]
        [Route("org/{id}/action/GetAuditLogById/{auditLogId}")]
        public IActionResult GetJobById(string id, string auditLogId)
        {
            var result = svc.GetAuditLogById(id, auditLogId);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/{id}/action/GetAuditLogs")]
        public IActionResult GetAuditLogs(string id, [FromBody] VMAuditLog param)
        {
            if (param.Limit <= 0)
            {
                param.Limit = 100;
            }

            var result = svc.GetAuditLogs(id, param);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/{id}/action/GetAuditLogCount")]
        public IActionResult GetAuditLogCount(string id, [FromBody] VMAuditLog param)
        {
            var result = svc.GetAuditLogCount(id, param);
            return Ok(result);
        }
    }
}
