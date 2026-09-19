using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Models;
using Its.Onix.Api.Services;
using Its.Onix.Api.ViewsModels;
using Its.Onix.Api.Database.Repositories;

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
        public async Task<IActionResult> GetJobById(string id, string auditLogId)
        {
            var result = await svc.GetAuditLogById(id, auditLogId);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/{id}/action/GetAuditLogs")]
        public async Task<IActionResult> GetAuditLogs(string id, [FromBody] VMAuditLog param)
        {
            if (param.Limit <= 0) param.Limit = 100;
            var result = await svc.GetAuditLogs(id, param);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/{id}/action/GetAuditLogCount")]
        public async Task<IActionResult> GetAuditLogCount(string id, [FromBody] VMAuditLog param)
        {
            var result = await svc.GetAuditLogCount(id, param);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/{id}/action/QueryAuditLogs")]
        public async Task<IActionResult> QueryAuditLogs(string id, [FromBody] VMAuditLog param)
        {
            if (param.Limit <= 0) param.Limit = 100;

            var total = await svc.GetAuditLogCount(id, param);
            var agg = await svc.GetAuditLogAggregations(id, param);

            var result = new VMAuditLogQueryResult
            {
                Total = total,
                Aggregations = agg,
            };

            if (param.ReturnDocs)
            {
                var logs = await svc.GetAuditLogs(id, param);
                result.Data = logs.Select(MapToEsFormat).ToList();
            }

            return Ok(result);
        }

        [HttpPost]
        [Route("org/{id}/action/GetScanItemsHistory")]
        public async Task<IActionResult> GetScanItemsHistory(string id, [FromBody] VMAuditLog param)
        {
            if (param.Limit <= 0) param.Limit = 50;

            var total = await svc.GetScanHistoryCount(id, param);
            var logs = await svc.GetScanHistory(id, param);

            var result = new
            {
                total,
                limit = param.Limit,
                offset = param.Offset,
                items = logs.Select(MapToScanHistoryFormat).ToList(),
            };

            return Ok(result);
        }

        [HttpPost]
        [Route("org/{id}/action/GetScanTimeline")]
        public async Task<IActionResult> GetScanTimeline(string id, [FromBody] VMAuditLog param)
        {
            var timeline = await svc.GetScanTimeline(id, param);

            var result = new
            {
                data = timeline.Buckets.Select(b => new
                {
                    timestamp = b.Timestamp,
                    total = b.Total,
                    productCounts = b.ProductCounts,
                }),
                interval = timeline.Interval,
                total = new { value = timeline.Total, relation = "eq" },
            };

            return Ok(result);
        }

        private static Dictionary<string, object?> MapToScanHistoryFormat(MAuditLog log)
        {
            var data = new Dictionary<string, object?>
            {
                ["@timestamp"] = log.CreatedDate?.ToString("O"),
                ["ContextData"] = new Dictionary<string, object?>
                {
                    ["Serial"] = log.Serial ?? AuditLogRepository.ExtractContextField(log.RawData, "Serial"),
                    ["Pin"] = log.Pin ?? AuditLogRepository.ExtractContextField(log.RawData, "Pin"),
                    ["CustomerEmail"] = AuditLogRepository.ExtractContextField(log.RawData, "CustomerEmail"),
                    ["ProductCode"] = AuditLogRepository.ExtractContextField(log.RawData, "ProductCode"),
                    ["FolderName"] = AuditLogRepository.ExtractContextField(log.RawData, "FolderName"),
                },
            };

            return new Dictionary<string, object?>
            {
                ["id"] = log.Id?.ToString(),
                ["index"] = "audit-logs",
                ["source"] = new Dictionary<string, object?>
                {
                    ["data"] = data,
                    // geoip is not available from Postgres - the frontend already falls back
                    // to "-" when country/city are missing.
                    ["geoip"] = new Dictionary<string, object?>(),
                    ["@timestamp"] = log.CreatedDate?.ToString("O"),
                },
            };
        }

        private static Dictionary<string, object?> MapToEsFormat(MAuditLog log)
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
                ["user_name"] = log.UserName,
                ["id_type"] = log.IdentityType,
                ["role"] = log.Role,
                ["action"] = log.ApiName,
                ["path"] = log.Path,
                ["resource"] = log.ControllerName,
                ["status_code"] = log.StatusCode,
                ["client_ip"] = log.ClientIp,
                ["geoip"] = AuditLogRepository.ExtractGeoIp(log.RawData),
                ["raw_data"] = log.RawData,
                ["data"] = data,
            };
        }
    }
}
