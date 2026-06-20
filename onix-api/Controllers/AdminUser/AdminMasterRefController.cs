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
    public class AdminMasterRefController : ControllerBase
    {
        private readonly IMasterRefService svc;

        [ExcludeFromCodeCoverage]
        public AdminMasterRefController(IMasterRefService service)
        {
            svc = service;
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/AddMasterRef")]
        public async Task<IActionResult> AddMasterRef([FromBody] MMasterRef request)
        {
            var result = await svc.AddMasterRefV2("global", request);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpDelete]
        [Route("org/global/action/DeleteMasterRefById/{masterRefId}")]
        public async Task<IActionResult> DeleteMasterRefById(string masterRefId)
        {
            var result = await svc.DeleteMasterRefByIdV2("global", masterRefId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/UpdateMasterRefById/{masterRefId}")]
        public async Task<IActionResult> UpdateMasterRefById(string masterRefId, [FromBody] MMasterRef request)
        {
            var result = await svc.UpdateMasterRefByIdV2("global", masterRefId, request);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/global/action/GetMasterRefById/{masterRefId}")]
        public async Task<IActionResult> GetMasterRefById(string masterRefId)
        {
            var result = await svc.GetMasterRefByIdV2("global", masterRefId);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/global/action/GetMasterRefs")]
        public async Task<IActionResult> GetMasterRefs([FromBody] VMMasterRef request)
        {
            if (request.Limit <= 0)
            {
                request.Limit = 100;
            }

            var result = await svc.GetMasterRefsV2("global", request);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/global/action/GetMasterRefCount")]
        public async Task<IActionResult> GetMasterRefCount([FromBody] VMMasterRef request)
        {
            var result = await svc.GetMasterRefCountV2("global", request);
            return Ok(result);
        }
    }
}
