using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Models;
using Its.Onix.Api.Services;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Controllers
{
    [Authorize(Policy = "GenericRolePolicy")]
    [ApiController]
    [Route("/admin-api/[controller]")]
    public class AdminIocController : ControllerBase
    {
        private readonly IIocService svc;

        [ExcludeFromCodeCoverage]
        public AdminIocController(IIocService service)
        {
            svc = service;
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/AddIoc")]
        public async Task<IActionResult> AddIoc([FromBody] MIoc request)
        {
            var result = await svc.AddIocV2("global", request);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpDelete]
        [Route("org/global/action/DeleteIocById/{iocId}")]
        public async Task<IActionResult> DeleteIocById(string iocId)
        {
            var result = await svc.DeleteIocByIdV2("global", iocId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/UpdateIocById/{iocId}")]
        public async Task<IActionResult> UpdateIocById(string iocId, [FromBody] MIoc request)
        {
            var result = await svc.UpdateIocByIdV2("global", iocId, request);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/EnableIocById/{iocId}")]
        public async Task<IActionResult> EnableIocById(string iocId)
        {
            var result = await svc.UpdateIocStatusByIdV2("global", iocId, "Active");
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/DisableIocById/{iocId}")]
        public async Task<IActionResult> DisableIocById(string iocId)
        {
            var result = await svc.UpdateIocStatusByIdV2("global", iocId, "Disabled");
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/global/action/GetIocById/{iocId}")]
        public async Task<IActionResult> GetIocById(string iocId)
        {
            var result = await svc.GetIocByIdV2("global", iocId);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/global/action/GetIocs")]
        public async Task<IActionResult> GetIocs([FromBody] VMIoc request)
        {
            if (request.Limit <= 0)
            {
                request.Limit = 100;
            }

            var result = await svc.GetIocsV2("global", request);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/global/action/GetIocCount")]
        public async Task<IActionResult> GetIocCount([FromBody] VMIoc request)
        {
            var result = await svc.GetIocCountV2("global", request);
            return Ok(result);
        }
    }
}
