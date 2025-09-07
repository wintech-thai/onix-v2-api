using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Models;
using Its.Onix.Api.Services;
using Its.Onix.Api.ViewsModels;
using Its.Onix.Api.ModelsViews;
using System.Text.Json;

namespace Its.Onix.Api.Controllers
{
    [Authorize(Policy = "GenericRolePolicy")]
    [ApiController]
    [Route("/api/[controller]")]
    public class JobController : ControllerBase
    {
        private readonly IJobService svc;

        [ExcludeFromCodeCoverage]
        public JobController(IJobService service)
        {
            svc = service;
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/CreateScanItemGeneratorJob")]
        public MVJob? CreateScanItemGeneratorJob(string id, [FromBody] MJob request)
        {
            request.Type = "ScanItemGenerator";
            request.Configuration = JsonSerializer.Serialize(request.Parameters);

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
