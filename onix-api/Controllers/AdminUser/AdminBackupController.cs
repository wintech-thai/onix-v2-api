using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
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
            param.EventTypeSet = new[] { "Backup.Schedule", "Backup.Adhoc", "Backup.Restore" };
            var result = _jobSvc.GetJobs("global", param);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/GetBackupJobCount")]
        public IActionResult GetBackupJobCount([FromBody] VMJob param)
        {
            param.EventTypeSet = new[] { "Backup.Schedule", "Backup.Adhoc", "Backup.Restore" };
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
        [HttpGet]
        [Route("org/global/action/GetBackupJobById/{jobId}")]
        public IActionResult GetBackupJobById(string jobId)
        {
            var result = _jobSvc.GetJobById("global", jobId);
            if (result == null) return NotFound();
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

        // ─── Item 4: List backup files from S3-compatible storage ───────────────

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/global/action/ListBackupFiles")]
        public async Task<IActionResult> ListBackupFiles()
        {
            var policyResult = await _configSvc.GetBackupPolicy("global");
            var policy = policyResult?.Configuration?.BackupPolicy;
            if (policy == null || string.IsNullOrEmpty(policy.StorageUrl))
                return BadRequest("Backup policy not configured");

            try
            {
                var files = await ListS3FilesAsync(policy);
                return Ok(files);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // ─── Item 3: Trigger restore from a backup file ──────────────────────────

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/TriggerRestore")]
        public IActionResult TriggerRestore([FromBody] VMRestoreRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Filename))
                return BadRequest("Filename is required");

            var job = new MJob
            {
                Type = "Backup.Restore",
                Name = $"Restore {DateTime.UtcNow:yyyyMMddHHmmss}",
                Description = $"Restore from {request.Bucket}/{request.Filename}",
                Tags = "backup,restore",
                Parameters =
                [
                    new NameValue { Name = "FILENAME", Value = request.Filename },
                    new NameValue { Name = "BUCKET",   Value = request.Bucket ?? "" },
                    new NameValue { Name = "FOLDER",   Value = request.Folder ?? "" },
                ]
            };

            var result = _jobSvc.AddJob("global", job);
            return Ok(result);
        }

        // ─── Private helpers ─────────────────────────────────────────────────────

        private static async Task<List<object>> ListS3FilesAsync(MBackupPolicy policy)
        {
            var credentials = new BasicAWSCredentials(policy.StorageKey!, policy.StorageSecret!);
            var config = new AmazonS3Config
            {
                ServiceURL     = policy.StorageUrl!,
                ForcePathStyle = false,
            };

            using var s3 = new AmazonS3Client(credentials, config);

            var prefix  = BuildPrefix(policy.Path, policy.FilePrefix);
            var request = new ListObjectsV2Request
            {
                BucketName = policy.Bucket!,
                Prefix     = prefix,
            };

            var result = await s3.ListObjectsV2Async(request);

            return result.S3Objects
                .OrderByDescending(o => o.LastModified)
                .Select(o => (object)new
                {
                    key          = o.Key,
                    filename     = System.IO.Path.GetFileName(o.Key),
                    bucket       = policy.Bucket,
                    folder       = policy.Path?.Trim('/') ?? "",
                    size         = o.Size,
                    lastModified = o.LastModified,
                })
                .ToList();
        }

        private static string BuildPrefix(string? folder, string? filePrefix)
        {
            var fp = string.IsNullOrWhiteSpace(filePrefix) ? "please-payment" : filePrefix.Trim();
            if (string.IsNullOrWhiteSpace(folder)) return fp;
            return $"{folder.Trim('/')}/{fp}";
        }
    }
}
