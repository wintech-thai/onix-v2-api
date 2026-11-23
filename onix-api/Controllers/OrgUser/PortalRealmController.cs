using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Models;
using Its.Onix.Api.Services;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Controllers
{
    // Tenant เข้าถึง Realm ของตัวเองได้แค่ดู (เลือกใช้ตอนสร้าง user) และจัดการ Realm User เท่านั้น
    // ห้ามสร้าง/แก้ไข/ลบ Realm เอง - นั่นเป็นหน้าที่ของ Admin (ดู AdminPortalRealmController)
    [ApiController]
    [Authorize(Policy = "GenericRolePolicy")]
    [Route("/api/[controller]")]
    public class PortalRealmController : ControllerBase
    {
        private readonly IPortalRealmService svc;

        [ExcludeFromCodeCoverage]
        public PortalRealmController(IPortalRealmService service)
        {
            svc = service;
        }

        public class ResetPasswordRequest
        {
            public string? Password { get; set; }
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/GetRealms")]
        public async Task<IActionResult> GetRealms(string id, [FromBody] VMPortalRealm param)
        {
            var result = await svc.GetRealms(id, param);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/GetRealmCount")]
        public async Task<IActionResult> GetRealmCount(string id, [FromBody] VMPortalRealm param)
        {
            var result = await svc.GetRealmCount(id, param);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{id}/action/GetRealmById/{realmId}")]
        public async Task<IActionResult> GetRealmById(string id, string realmId)
        {
            var result = await svc.GetRealmById(id, realmId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/GetRealmUsers/{realmId}")]
        public async Task<IActionResult> GetRealmUsers(string id, string realmId, [FromBody] VMPortalRealmUser param)
        {
            var result = await svc.GetRealmUsers(id, realmId, param);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/GetRealmUserCount/{realmId}")]
        public async Task<IActionResult> GetRealmUserCount(string id, string realmId, [FromBody] VMPortalRealmUser param)
        {
            var result = await svc.GetRealmUserCount(id, realmId, param);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/AddRealmUser/{realmId}")]
        public async Task<IActionResult> AddRealmUser(string id, string realmId, [FromBody] MPortalRealmUser request)
        {
            var result = await svc.AddRealmUser(id, realmId, request);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/UpdateRealmUserById/{realmUserId}")]
        public async Task<IActionResult> UpdateRealmUserById(string id, string realmUserId, [FromBody] MPortalRealmUser request)
        {
            var result = await svc.UpdateRealmUserById(id, realmUserId, request);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/ResetRealmUserPasswordById/{realmUserId}")]
        public async Task<IActionResult> ResetRealmUserPasswordById(string id, string realmUserId, [FromBody] ResetPasswordRequest request)
        {
            var result = await svc.ResetRealmUserPasswordById(id, realmUserId, request.Password ?? "");
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/EnableRealmUserById/{realmUserId}")]
        public async Task<IActionResult> EnableRealmUserById(string id, string realmUserId)
        {
            var result = await svc.UpdateRealmUserStatusById(id, realmUserId, true);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/DisableRealmUserById/{realmUserId}")]
        public async Task<IActionResult> DisableRealmUserById(string id, string realmUserId)
        {
            var result = await svc.UpdateRealmUserStatusById(id, realmUserId, false);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpDelete]
        [Route("org/{id}/action/DeleteRealmUserById/{realmUserId}")]
        public async Task<IActionResult> DeleteRealmUserById(string id, string realmUserId)
        {
            var result = await svc.DeleteRealmUserById(id, realmUserId);
            return Ok(result);
        }
    }
}
