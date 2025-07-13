using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Models;
using Its.Onix.Api.Services;
using Its.Onix.Api.ViewsModels;
using Its.Onix.Api.ModelsViews;

namespace Its.Onix.Api.Controllers
{
    [Authorize(Policy = "GenericRolePolicy")]
    [ApiController]
    [Route("/api/[controller]")]
    public class MasterRefController : ControllerBase
    {
        private readonly IMasterRefService svc;

        [ExcludeFromCodeCoverage]
        public MasterRefController(IMasterRefService service)
        {
            svc = service;
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/AddMasterRef")]
        public MVMasterRef? AddMasterRef(string id, [FromBody] MMasterRef request)
        {
            var result = svc.AddMasterRef(id, request);
            return result;
        }

        [ExcludeFromCodeCoverage]
        [HttpDelete]
        [Route("org/{id}/action/DeleteMasterRefById/{masterRefId}")]
        public IActionResult DeleteMasterRefById(string id, string masterRefId)
        {
            var result = svc.DeleteMasterRefById(id, masterRefId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/UpdateMasterRefById/{masterRefId}")]
        public IActionResult UpdateMasterRefById(string id, string masterRefId, [FromBody] MMasterRef request)
        {
            var result = svc.UpdateMasterRefById(id, masterRefId, request);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{id}/action/GetMasterRefById/{masterRefId}")]
        public MMasterRef GetMasterRefById(string id, string masterRefId)
        {
            var result = svc.GetMasterRefById(id, masterRefId);
            return result;
        }

        [HttpPost]
        [Route("org/{id}/action/GetMasterRefs")]
        public IActionResult GetMasterRefs(string id, [FromBody] VMMasterRef param)
        {
            if (param.Limit <= 0)
            {
                param.Limit = 100;
            }

            var result = svc.GetMasterRefs(id, param);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/{id}/action/GetMasterRefCount")]
        public IActionResult GetMasterRefCount(string id, [FromBody] VMMasterRef param)
        {
            var result = svc.GetMasterRefCount(id, param);
            return Ok(result);
        }
    }
}
