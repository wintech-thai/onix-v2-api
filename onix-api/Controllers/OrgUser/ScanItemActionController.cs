using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Models;
using Its.Onix.Api.Services;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Controllers
{
    [Authorize(Policy = "GenericRolePolicy")]
    [ApiController]
    [Route("/api/[controller]")]
    public class ScanItemActionController : ControllerBase
    {
        private readonly IScanItemActionService svc;

        public ScanItemActionController(IScanItemActionService service)
        {
            svc = service;
        }

        [HttpGet]
        [Route("org/{id}/action/GetScanItemAction")]
        public async Task<IActionResult> GetScanItemAction(string id)
        {
            var result = await svc.GetScanItemAction_V2(id);
            return Ok(result);
        }

        [HttpGet]
        [Route("org/{id}/action/GetScanItemActionDefault")]
        public IActionResult GetScanItemActionDefault(string id)
        {
            var result = svc.GetScanItemActionDefault_V2(id);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/{id}/action/AddScanItemAction")]
        public async Task<IActionResult> AddScanItemAction(string id, [FromBody] MScanItemAction request)
        {
            var result = await svc.AddScanItemAction_V2(id, request);
            Response.Headers.Append("CUST_STATUS", result!.Status);

            return Ok(result);
        }

        [HttpPost]
        [Route("org/{id}/action/UpdateScanItemActionById/{actionId}")]
        public async Task<IActionResult> UpdateScanItemActionById(string id, string actionId, [FromBody] MScanItemAction request)
        {
            var result = await svc.UpdateScanItemActionById_V2(id, actionId, request);
            Response.Headers.Append("CUST_STATUS", result!.Status);

            return Ok(result);
        }

        [HttpPost]
        [Route("org/{id}/action/GetScanItemActions")]
        public async Task<IActionResult> GetScanItemActions(string id, [FromBody] VMScanItemAction request)
        {
            var result = await svc.GetScanItemActions_V2(id, request);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/{id}/action/GetScanItemActionCount")]
        public async Task<IActionResult> GetScanItemActionCount(string id, [FromBody] VMScanItemAction request)
        {
            var result = await svc.GetScanItemActionCount_V2(id, request);
            return Ok(result);
        }

        [HttpDelete]
        [Route("org/{id}/action/DeleteScanItemActionById/{scanItemActionId}")]
        public async Task<IActionResult> DeleteScanItemActionById(string id, string scanItemActionId)
        {
            var result = await svc.DeleteScanItemActionById_V2(id, scanItemActionId);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/{id}/action/SetDefaultScanItemActionById/{actionId}")]
        public async Task<IActionResult> SetDefaultScanItemActionById(string id, string actionId)
        {
            var result = await svc.SetDefaultScanItemActionById_V2(id, actionId);
            Response.Headers.Append("CUST_STATUS", result!.Status);

            return Ok(result);
        }
    }
}
