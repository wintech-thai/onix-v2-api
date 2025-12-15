using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Models;
using Its.Onix.Api.Services;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Controllers
{
    [Authorize(Policy = "GenericRolePolicy")]
    [ApiController]
    [Route("/api/[controller]")]
    public class ScanItemTemplateController : ControllerBase
    {
        private readonly IScanItemTemplateService svc;

        public ScanItemTemplateController(IScanItemTemplateService service)
        {
            svc = service;
        }

        [HttpGet]
        [Route("org/{id}/action/GetScanItemTemplate")]
        public async Task<IActionResult> GetScanItemTemplate(string id)
        {
            var result = await svc.GetScanItemTemplate_V2(id);
            return Ok(result);
        }

        [HttpGet]
        [Route("org/{id}/action/GetScanItemTemplateDefault")]
        public IActionResult GetScanItemTemplateDefault(string id)
        {
            var userName = "";
            var nameObj = Response.HttpContext.Items["Temp-Identity-Name"];
            if (nameObj != null)
            {
                userName = nameObj.ToString()!;;
            }

            var result = svc.GetScanItemTemplateDefault(id, userName);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/{id}/action/AddScanItemTemplate")]
        public async Task<IActionResult> AddScanItemTemplate(string id, [FromBody] MScanItemTemplate request)
        {
            var result = await svc.AddScanItemTemplate_V2(id, request);
            Response.Headers.Append("CUST_STATUS", result!.Status);

            return Ok(result);
        }

        [HttpPost]
        [Route("org/{id}/action/UpdateScanItemTemplateById/{templateId}")]
        public async Task<IActionResult> UpdateScanItemTemplateById(string id, string templateId, [FromBody] MScanItemTemplate request)
        {
            var result = await svc.UpdateScanItemTemplateById_V2(id, templateId, request);
            Response.Headers.Append("CUST_STATUS", result!.Status);

            return Ok(result);
        }

        [HttpGet]
        [Route("org/{id}/action/GetScanItemTemplateById/{templateId}")]
        public async Task<IActionResult> GetScanItemTemplateById(string id, string templateId)
        {
            var result = await svc.GetScanItemTemplateById_V2(id, templateId);
            Response.Headers.Append("CUST_STATUS", result!.Status);

            return Ok(result);
        }

        [HttpPost]
        [Route("org/{id}/action/GetScanItemTemplates")]
        public async Task<IActionResult> GetScanItemTemplates(string id, [FromBody] VMScanItemTemplate request)
        {
            var result = await svc.GetScanItemTemplates_V2(id, request);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/{id}/action/GetScanItemTemplateCount")]
        public async Task<IActionResult> GetScanItemTemplateCount(string id, [FromBody] VMScanItemTemplate request)
        {
            var result = await svc.GetScanItemTemplateCount_V2(id, request);
            return Ok(result);
        }

        [HttpDelete]
        [Route("org/{id}/action/DeleteScanItemTemplateById/{scanItemActionId}")]
        public async Task<IActionResult> DeleteScanItemTemplateById(string id, string scanItemActionId)
        {
            var result = await svc.DeleteScanItemTemplateById_V2(id, scanItemActionId);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/{id}/action/SetDefaultScanItemTemplateById/{templateId}")]
        public async Task<IActionResult> SetDefaultScanItemTemplateById(string id, string templateId)
        {
            var result = await svc.SetDefaultScanItemTemplateById_V2(id, templateId);
            Response.Headers.Append("CUST_STATUS", result!.Status);

            return Ok(result);
        }

        [HttpGet]
        [Route("org/{id}/action/GetJobDefaultByTemplateId/{templateId}")]
        public async Task<IActionResult> GetJobDefaultByTemplateId(string id, string templateId)
        {
            var result = await svc.GetJobDefaultByTemplateId(id, "ScanItemGenerator", templateId);
            Response.Headers.Append("CUST_STATUS", result!.Status);

            return Ok(result);
        }
    }
}
