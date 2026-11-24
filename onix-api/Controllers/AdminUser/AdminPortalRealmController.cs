using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Models;
using Its.Onix.Api.Services;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Controllers
{
    // Admin ต้องจัดการ Realm ของ tenant ที่ตัวเองเลือกอยู่ แต่ไม่มี OrgUser token ของ tenant นั้น
    // เลยต้องผ่าน /admin-api/.../org/global แล้วส่ง orgId จริงมาทาง route param แทน (เหมือน AdminPortalPageController)
    [ApiController]
    [Authorize(Policy = "GenericRolePolicy")]
    [Route("/admin-api/[controller]")]
    public class AdminPortalRealmController : ControllerBase
    {
        private readonly IPortalRealmService svc;

        [ExcludeFromCodeCoverage]
        public AdminPortalRealmController(IPortalRealmService service)
        {
            svc = service;
        }

        public class ResetPasswordRequest
        {
            public string? Password { get; set; }
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/GetRealms/{orgId}")]
        public async Task<IActionResult> GetRealms(string orgId, [FromBody] VMPortalRealm param)
        {
            var result = await svc.GetRealms(orgId, param);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/GetRealmCount/{orgId}")]
        public async Task<IActionResult> GetRealmCount(string orgId, [FromBody] VMPortalRealm param)
        {
            var result = await svc.GetRealmCount(orgId, param);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/global/action/GetRealmById/{orgId}/{realmId}")]
        public async Task<IActionResult> GetRealmById(string orgId, string realmId)
        {
            var result = await svc.GetRealmById(orgId, realmId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/AddRealm/{orgId}")]
        public async Task<IActionResult> AddRealm(string orgId, [FromBody] MPortalRealm request)
        {
            var result = await svc.AddRealm(orgId, request);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/UpdateRealmById/{orgId}/{realmId}")]
        public async Task<IActionResult> UpdateRealmById(string orgId, string realmId, [FromBody] MPortalRealm request)
        {
            var result = await svc.UpdateRealmById(orgId, realmId, request);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpDelete]
        [Route("org/global/action/DeleteRealmById/{orgId}/{realmId}")]
        public async Task<IActionResult> DeleteRealmById(string orgId, string realmId)
        {
            var result = await svc.DeleteRealmById(orgId, realmId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/GetRealmUsers/{orgId}/{realmId}")]
        public async Task<IActionResult> GetRealmUsers(string orgId, string realmId, [FromBody] VMPortalRealmUser param)
        {
            var result = await svc.GetRealmUsers(orgId, realmId, param);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/GetRealmUserCount/{orgId}/{realmId}")]
        public async Task<IActionResult> GetRealmUserCount(string orgId, string realmId, [FromBody] VMPortalRealmUser param)
        {
            var result = await svc.GetRealmUserCount(orgId, realmId, param);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/global/action/GetRealmUserById/{orgId}/{realmUserId}")]
        public async Task<IActionResult> GetRealmUserById(string orgId, string realmUserId)
        {
            var result = await svc.GetRealmUserById(orgId, realmUserId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/AddRealmUser/{orgId}/{realmId}")]
        public async Task<IActionResult> AddRealmUser(string orgId, string realmId, [FromBody] MPortalRealmUser request)
        {
            var result = await svc.AddRealmUser(orgId, realmId, request);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/UpdateRealmUserById/{orgId}/{realmUserId}")]
        public async Task<IActionResult> UpdateRealmUserById(string orgId, string realmUserId, [FromBody] MPortalRealmUser request)
        {
            var result = await svc.UpdateRealmUserById(orgId, realmUserId, request);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/ResetRealmUserPasswordById/{orgId}/{realmUserId}")]
        public async Task<IActionResult> ResetRealmUserPasswordById(string orgId, string realmUserId, [FromBody] ResetPasswordRequest request)
        {
            var result = await svc.ResetRealmUserPasswordById(orgId, realmUserId, request.Password ?? "");
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/EnableRealmUserById/{orgId}/{realmUserId}")]
        public async Task<IActionResult> EnableRealmUserById(string orgId, string realmUserId)
        {
            var result = await svc.UpdateRealmUserStatusById(orgId, realmUserId, true);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/DisableRealmUserById/{orgId}/{realmUserId}")]
        public async Task<IActionResult> DisableRealmUserById(string orgId, string realmUserId)
        {
            var result = await svc.UpdateRealmUserStatusById(orgId, realmUserId, false);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpDelete]
        [Route("org/global/action/DeleteRealmUserById/{orgId}/{realmUserId}")]
        public async Task<IActionResult> DeleteRealmUserById(string orgId, string realmUserId)
        {
            var result = await svc.DeleteRealmUserById(orgId, realmUserId);
            return Ok(result);
        }
    }
}
