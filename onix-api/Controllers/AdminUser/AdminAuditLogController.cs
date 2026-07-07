using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Services;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Controllers
{
    [Authorize(Policy = "GenericRolePolicy")]
    [ApiController]
    [Route("/admin-api/[controller]")]
    public class AdminAuditLogController : ControllerBase
    {
        private readonly IAuditLogService svc;

        [ExcludeFromCodeCoverage]
        public AdminAuditLogController(IAuditLogService service)
        {
            svc = service;
        }

        [HttpGet]
        [Route("org/global/action/GetAuditLogById/{auditLogId}")]
        public IActionResult GetAuditLogById(string auditLogId)
        {
            var log = svc.GetAllAuditLogById(auditLogId);
            if (log == null) return NotFound();
            return Ok(MapToEsFormat(log));
        }

        [HttpPost]
        [Route("org/global/action/GetAuditLogCount")]
        public IActionResult GetAuditLogCount([FromBody] VMAuditLog param)
        {
            var count = svc.GetAllAuditLogCount(param);
            return Ok(count);
        }

        // Main query endpoint — returns documents + aggregations in ES-compatible format
        [HttpPost]
        [Route("org/global/action/QueryAuditLogs")]
        public IActionResult QueryAuditLogs([FromBody] VMAuditLog param)
        {
            if (param.Limit <= 0) param.Limit = 100;

            var result = new VMAuditLogQueryResult
            {
                Total = svc.GetAllAuditLogCount(param),
                Aggregations = svc.GetAllAuditLogAggregations(param),
            };

            if (param.ReturnDocs)
            {
                var logs = svc.GetAllAuditLogs(param);
                result.Data = logs.Select(MapToEsFormat).ToList();
            }

            return Ok(result);
        }

        private static Dictionary<string, object?> MapToEsFormat(Its.Onix.Api.Models.MAuditLog log)
        {
            var api = new Dictionary<string, object?>
            {
                ["ApiName"] = log.ApiName,
                ["Controller"] = log.ControllerName,
                ["OrgId"] = log.OrgId,
            };

            var userInfo = new Dictionary<string, object?>
            {
                ["UserName"] = log.UserName,
                ["Role"] = log.Role,
                ["IdentityType"] = log.IdentityType,
                ["UserId"] = log.UserId,
            };

            var data = new Dictionary<string, object?>
            {
                ["ClientIp"] = log.ClientIp,
                ["CfClientIp"] = log.CfClientIp,
                ["StatusCode"] = log.StatusCode,
                ["Path"] = log.Path,
                ["QueryString"] = log.QueryString,
                ["UserAgent"] = log.UserAgent,
                ["Host"] = log.Host,
                ["Environment"] = log.Environment,
                ["CustomStatus"] = log.CustomStatus,
                ["CustomDesc"] = log.CustomDesc,
                ["RequestSize"] = log.RequestSize,
                ["ResponseSize"] = log.ResponseSize,
                ["LatencyMs"] = log.LatencyMs,
                ["api"] = api,
                ["userInfo"] = userInfo,
            };

            return new Dictionary<string, object?>
            {
                ["_id"] = log.Id?.ToString(),
                ["id"] = log.Id?.ToString(),
                ["@timestamp"] = log.CreatedDate?.ToString("O"),
                ["raw_data"] = log.RawData,
                ["data"] = data,
            };
        }
    }
}
