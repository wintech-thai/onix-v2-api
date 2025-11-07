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
    public class CycleController : ControllerBase
    {
        private readonly ICycleService svc;

        [ExcludeFromCodeCoverage]
        public CycleController(ICycleService service)
        {
            svc = service;
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/AddCycle")]
        public MVCycle? AddCycle(string id, [FromBody] MCycle request)
        {
            var result = svc.AddCycle(id, request);
            return result;
        }

        [ExcludeFromCodeCoverage]
        [HttpDelete]
        [Route("org/{id}/action/DeleteCycleById/{cycleId}")]
        public IActionResult DeleteCycleById(string id, string cycleId)
        {
            var result = svc.DeleteCycleById(id, cycleId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/UpdateCycleById/{cycleId}")]
        public IActionResult UpdateCycleById(string id, string cycleId, [FromBody] MCycle request)
        {
            var result = svc.UpdateCycleById(id, cycleId, request);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{id}/action/GetCycleById/{cycleId}")]
        public MCycle GetCycleById(string id, string cycleId)
        {
            var result = svc.GetCycleById(id, cycleId);
            return result;
        }

        [HttpPost]
        [Route("org/{id}/action/GetCycles")]
        public IActionResult GetCycles(string id, [FromBody] VMCycle param)
        {
            if (param.Limit <= 0)
            {
                param.Limit = 100;
            }

            var result = svc.GetCycles(id, param);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/{id}/action/GetCycleCount")]
        public IActionResult GetCycleCount(string id, [FromBody] VMCycle param)
        {
            var result = svc.GetCycleCount(id, param);
            return Ok(result);
        }
    }
}
