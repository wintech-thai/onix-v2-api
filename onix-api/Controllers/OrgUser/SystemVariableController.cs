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
    public class SystemVariableController : ControllerBase
    {
        private readonly ISystemVariableService svc;

        [ExcludeFromCodeCoverage]
        public SystemVariableController(ISystemVariableService service)
        {
            svc = service;
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/AddSystemVariable")]
        public MVSystemVariable? AddSystemVariable(string id, [FromBody] MSystemVariable request)
        {
            var result = svc.AddSystemVariable(id, request);
            return result;
        }

        [ExcludeFromCodeCoverage]
        [HttpDelete]
        [Route("org/{id}/action/DeleteSystemVariableById/{systemVariableId}")]
        public IActionResult DeleteSystemVariableById(string id, string systemVariableId)
        {
            var result = svc.DeleteSystemVariableById(id, systemVariableId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/UpdateSystemVariableById/{systemVariableId}")]
        public IActionResult UpdateSystemVariableById(string id, string systemVariableId, [FromBody] MSystemVariable request)
        {
            var result = svc.UpdateSystemVariableById(id, systemVariableId, request);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{id}/action/GetSystemVariableById/{systemVariableId}")]
        public MSystemVariable GetSystemVariableById(string id, string systemVariableId)
        {
            var result = svc.GetSystemVariableById(id, systemVariableId);
            return result;
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{id}/action/GetSystemVariableByName/{name}")]
        public MSystemVariable GetSystemVariableByName(string id, string name)
        {
            var result = svc.GetSystemVariableByName(id, name);
            return result;
        }

        [HttpPost]
        [Route("org/{id}/action/GetSystemVariables")]
        public IActionResult GetSystemVariables(string id, [FromBody] VMSystemVariable param)
        {
            if (param.Limit <= 0)
            {
                param.Limit = 100;
            }

            var result = svc.GetSystemVariables(id, param);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/{id}/action/GetSystemVariableCount")]
        public IActionResult GetSystemVariableCount(string id, [FromBody] VMSystemVariable param)
        {
            var result = svc.GetSystemVariableCount(id, param);
            return Ok(result);
        }
    }
}
