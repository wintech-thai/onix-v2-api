using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Services;
using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Controllers
{
    [Authorize(Policy = "GenericRolePolicy")]
    [ApiController]
    [Route("/admin-api/[controller]")]
    public class AdminServiceRequestController : ControllerBase
    {
        private readonly IServiceRequestService svc;
        private readonly IJobService _jobSvc;

        [ExcludeFromCodeCoverage]
        public AdminServiceRequestController(IServiceRequestService service, IJobService jobService)
        {
            svc = service;
            _jobSvc = jobService;
        }

        // x089 7.a.i : เรียกจาก please-payment.com ผ่าน API key (GenericRolePolicy) — สร้าง
        // Pending service request + ส่ง link ไปทาง email
        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/CreatePleasePaymentRegistrationLink")]
        public async Task<IActionResult> CreatePleasePaymentRegistrationLink([FromBody] MServiceRequest request)
        {
            var result = await svc.CreatePleasePaymentRegistrationLink(request);
            return Ok(result);
        }

        // x089 7.a.ii : ลูกค้าเปิดจาก email แล้วกรอกฟอร์ม — ไม่มี authen, ใช้ requestId+otp แทน
        [ExcludeFromCodeCoverage]
        [AllowAnonymous]
        [HttpPost]
        [Route("org/global/action/SavePleasePaymentServiceRequest/{requestId}/{otp}")]
        public async Task<IActionResult> SavePleasePaymentServiceRequest(string requestId, string otp, [FromBody] MServiceRequest request)
        {
            var result = await svc.ConfirmPleasePaymentServiceRequest(requestId, otp, request);
            return Ok(result);
        }

        // ให้หน้า customer-signup (Admin) เรียกตอนเปิดหน้าเพื่อเช็คสถานะ + prefill ข้อมูล — ไม่มี authen เช่นกัน
        [ExcludeFromCodeCoverage]
        [AllowAnonymous]
        [HttpGet]
        [Route("org/global/action/GetPleasePaymentServiceRequestByRequestIdOtp/{requestId}/{otp}")]
        public async Task<IActionResult> GetPleasePaymentServiceRequestByRequestIdOtp(string requestId, string otp)
        {
            var result = await svc.GetServiceRequestByIdAndOtp(requestId, otp);
            return Ok(result);
        }

        // อัปโหลดรูป book bank ระหว่างกรอกฟอร์ม — ไม่มี authen, ยืนยันสิทธิ์ด้วย requestId+otp เหมือนกัน
        [ExcludeFromCodeCoverage]
        [AllowAnonymous]
        [HttpPost]
        [Route("org/global/action/UploadPleasePaymentServiceRequestBookBank/{requestId}/{otp}")]
        public async Task<IActionResult> UploadPleasePaymentServiceRequestBookBank(string requestId, string otp, [FromBody] MFileDocument fileDocument)
        {
            var result = await svc.UploadServiceRequestBookBank(requestId, otp, fileDocument);
            return Ok(result);
        }

        // x089 7.a.iii : CRUD ปกติ ฝั่ง Admin (มี authen)
        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/global/action/GetServiceRequestById/{serviceRequestId}")]
        public async Task<IActionResult> GetServiceRequestById(string serviceRequestId)
        {
            var result = await svc.GetServiceRequestById(serviceRequestId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/GetServiceRequests")]
        public async Task<IActionResult> GetServiceRequests([FromBody] VMServiceRequest param)
        {
            var result = await svc.GetServiceRequests(param);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/GetServiceRequestCount")]
        public async Task<IActionResult> GetServiceRequestCount([FromBody] VMServiceRequest param)
        {
            var result = await svc.GetServiceRequestCount(param);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/UpdateServiceRequestById/{serviceRequestId}")]
        public async Task<IActionResult> UpdateServiceRequestById(string serviceRequestId, [FromBody] MServiceRequest request)
        {
            var result = await svc.UpdateServiceRequestById(serviceRequestId, request);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpDelete]
        [Route("org/global/action/DeleteServiceRequestById/{serviceRequestId}")]
        public async Task<IActionResult> DeleteServiceRequestById(string serviceRequestId)
        {
            var result = await svc.DeleteServiceRequestById(serviceRequestId);
            return Ok(result);
        }

        // ปุ่ม "Regenerate Link" ฝั่ง Admin
        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/RegenerateServiceRequestLink/{serviceRequestId}")]
        public async Task<IActionResult> RegenerateServiceRequestLink(string serviceRequestId, [FromQuery] string? adminDomain)
        {
            var result = await svc.RegenerateRegistrationLink(serviceRequestId, adminDomain);
            return Ok(result);
        }

        // ปุ่ม "Resend Email" ฝั่ง Admin
        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/ResendServiceRequestLinkEmail/{serviceRequestId}")]
        public async Task<IActionResult> ResendServiceRequestLinkEmail(string serviceRequestId)
        {
            var result = await svc.ResendRegistrationLinkEmail(serviceRequestId);
            return Ok(result);
        }

        // x089 9.e : ดู job log (เช่น ส่งอีเมลสำเร็จ/ล้มเหลว) ของ service request หนึ่ง ๆ — อาจมีหลาย job
        // ต่อ 1 request (เช่น resend link) จึงคืนเป็น list เสมอ
        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/global/action/GetServiceRequestJobs/{serviceRequestId}")]
        public IActionResult GetServiceRequestJobs(string serviceRequestId)
        {
            var result = _jobSvc.GetJobs("global", new VMJob { RefId = serviceRequestId, Limit = 100 });

            // x089 ปรับแก้ 7.b : Parameters is [NotMapped] — AddJob() serializes it into Configuration
            // on write, but nothing deserializes it back on read, so rehydrate it here for the
            // admin UI to pull REGISTRATION_LINK out of without needing to parse Configuration itself
            foreach (var job in result)
            {
                if (string.IsNullOrEmpty(job.Configuration)) continue;
                try
                {
                    job.Parameters = JsonSerializer.Deserialize<List<NameValue>>(job.Configuration) ?? new();
                }
                catch (JsonException) { }
            }

            return Ok(result);
        }
    }
}
