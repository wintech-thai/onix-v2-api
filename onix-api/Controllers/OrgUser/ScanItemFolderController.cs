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
    public class ScanItemFolderController : ControllerBase
    {
        private readonly IScanItemFolderService svc;

        public ScanItemFolderController(IScanItemFolderService service)
        {
            svc = service;
        }

        [HttpPost]
        [Route("org/{id}/action/AddScanItemFolder")]
        public async Task<IActionResult> AddScanItemFolder(string id, [FromBody] MScanItemFolder request)
        {
            var result = await svc.AddScanItemFolder(id, request);
            Response.Headers.Append("CUST_STATUS", result!.Status);

            return Ok(result);
        }

        [HttpPost]
        [Route("org/{id}/action/UpdateScanItemFolderById/{folderId}")]
        public async Task<IActionResult> UpdateScanItemFolderById(string id, string folderId, [FromBody] MScanItemFolder request)
        {
            var result = await svc.UpdateScanItemFolderById(id, folderId, request);
            Response.Headers.Append("CUST_STATUS", result!.Status);

            return Ok(result);
        }

        [HttpGet]
        [Route("org/{id}/action/GetScanItemFolderById/{folderId}")]
        public async Task<IActionResult> GetScanItemFolderById(string id, string folderId)
        {
            var result = await svc.GetScanItemFolderById(id, folderId);
            Response.Headers.Append("CUST_STATUS", result!.Status);

            return Ok(result);
        }

        [HttpPost]
        [Route("org/{id}/action/GetScanItemFolders")]
        public async Task<IActionResult> GetScanItemFolders(string id, [FromBody] VMScanItemFolder request)
        {
            var result = await svc.GetScanItemFolders(id, request);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/{id}/action/GetScanItemFolderCount")]
        public async Task<IActionResult> GetScanItemFolderCount(string id, [FromBody] VMScanItemFolder request)
        {
            var result = await svc.GetScanItemFolderCount(id, request);
            return Ok(result);
        }

        [HttpDelete]
        [Route("org/{id}/action/DeleteScanItemFolderById/{scanItemActionId}")]
        public async Task<IActionResult> DeleteScanItemFolderById(string id, string scanItemActionId)
        {
            var result = await svc.DeleteScanItemFolderById(id, scanItemActionId);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/{id}/action/AttachScanItemFolderToAction/{folderId}/{actionId}")]
        public async Task<IActionResult> SetDefaultScanItemFolderById(string id, string folderId, string actionId)
        {
            var result = await svc.AttachScanItemFolderToAction(id, folderId, actionId);
            Response.Headers.Append("CUST_STATUS", result!.Status);

            return Ok(result);
        }
    }
}
