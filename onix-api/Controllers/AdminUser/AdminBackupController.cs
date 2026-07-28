using Its.Onix.Api.Models;
using Its.Onix.Api.Services;
using Its.Onix.Api.ViewsModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.Controllers
{
    [Authorize(Policy = "GenericRolePolicy")]
    [ApiController]
    [Route("/admin-api/[controller]")]
    public class AdminBackupController : ControllerBase
    {
        private readonly IJobService _jobSvc;
        private readonly IConfigurationService _configSvc;

        [ExcludeFromCodeCoverage]
        public AdminBackupController(IJobService jobService, IConfigurationService configService)
        {
            _jobSvc = jobService;
            _configSvc = configService;
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/GetBackupJobs")]
        public IActionResult GetBackupJobs([FromBody] VMJob param)
        {
            if (param.Limit <= 0) param.Limit = 100;
            param.JobType = "Backup.Schedule";
            var result = _jobSvc.GetJobs("global", param);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/GetBackupJobCount")]
        public IActionResult GetBackupJobCount([FromBody] VMJob param)
        {
            param.JobType = "Backup.Schedule";
            var result = _jobSvc.GetJobCount("global", param);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/global/action/GetBackupPolicy")]
        public async Task<IActionResult> GetBackupPolicy()
        {
            var result = await _configSvc.GetBackupPolicy("global");
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/SetBackupPolicy")]
        public async Task<IActionResult> SetBackupPolicy([FromBody] MConfiguration config)
        {
            var result = await _configSvc.SetBackupPolicy("global", config);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/TriggerBackupNow")]
        public IActionResult TriggerBackupNow()
        {
            var job = new MJob
            {
                Type = "Backup.Adhoc",
                Name = $"Adhoc Backup {DateTime.UtcNow:yyyyMMddHHmmss}",
                Description = "Manual backup triggered from admin UI",
                Tags = "backup,adhoc",
            };
            var result = _jobSvc.AddJob("global", job);
            return Ok(result);
        }
    }
}
