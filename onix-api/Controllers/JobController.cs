using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Models;
using Its.Onix.Api.Services;
using Its.Onix.Api.ViewsModels;
using Its.Onix.Api.ModelsViews;
using System.Net;

namespace Its.Onix.Api.Controllers
{
    [Authorize(Policy = "GenericRolePolicy")]
    [ApiController]
    [Route("/api/[controller]")]
    public class JobController : ControllerBase
    {
        private readonly IJobService svc;
        private readonly IScanItemTemplateService _scanItemTemplateService;

        [ExcludeFromCodeCoverage]
        public JobController(IJobService service, IScanItemTemplateService scanItemTemplateService)
        {
            svc = service;
            _scanItemTemplateService = scanItemTemplateService;
        }

        private NameValue[] ConfigDefaultParams(string orgId, MScanItemTemplate template, MJob job)
        {
            var exceptionFields = new string[] { "SCAN_ITEM_ORG", "SERIAL_NUMBER_DIGIT" }; /* Fields not allow to pass by user */

            var userParams = job.Parameters;
            var userFields = userParams.ToDictionary(item => item.Name!, item => item.Value);

            var customParams = new NameValue[]
            {
                new NameValue() { Name = "SCAN_ITEM_ORG", Value = orgId },
                new NameValue() { Name = "SCAN_ITEM_COUNT", Value = template.GetPropertyValue("GeneratorCount", "10") },
                new NameValue() { Name = "SCAN_ITEM_URL", Value = template.GetPropertyValue("UrlTemplate", "") },
                new NameValue() { Name = "EMAIL_NOTI_ADDRESS", Value = template.GetPropertyValue("NotificationEmail", "") },
                new NameValue() { Name = "SERIAL_NUMBER_DIGIT", Value = template.GetPropertyValue("SerialDigit", "7") },
            };

            foreach (var param in customParams)
            {
                var name = param.Name;
                if (exceptionFields.Contains(name)) continue;

                if (userFields.ContainsKey(name!))
                {
                    //Update with user provided value
                    param.Value = userFields[name!];
                }
            }

            return customParams;
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/CreateJobScanItemGenerator")]
        public MVJob? CreateJobScanItemGenerator(string id, [FromBody] MJob request)
        {
            var m = _scanItemTemplateService!.GetScanItemTemplate(id);
            if (m == null)
            {
                Response.Headers.Append("CUST_STATUS", "NO_SCAN_ITEM_TEMPLATE");
            }

            var customParams = ConfigDefaultParams(id, m!, request);
            request.Parameters = customParams;

            request.Type = "ScanItemGenerator";
            request.Status = "Pending";

            var result = svc.AddJob(id, request);
            return result;
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{id}/action/GetJobById/{jobId}")]
        public MJob GetJobById(string id, string jobId)
        {
            var result = svc.GetJobById(id, jobId);
            return result;
        }

        [HttpPost]
        [Route("org/{id}/action/GetJobs")]
        public IActionResult GetJobs(string id, [FromBody] VMJob param)
        {
            if (param.Limit <= 0)
            {
                param.Limit = 100;
            }

            var result = svc.GetJobs(id, param);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/{id}/action/GetJobCount")]
        public IActionResult GetJobCount(string id, [FromBody] VMJob param)
        {
            var result = svc.GetJobCount(id, param);
            return Ok(result);
        }
    }
}
