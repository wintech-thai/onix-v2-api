using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Models;
using Its.Onix.Api.Services;
using Its.Onix.Api.ModelsViews;

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
        public MScanItemTemplate GetScanItemTemplate(string id)
        {
            var result = svc.GetScanItemTemplate(id);
            return result;
        }

        [HttpGet]
        [Route("org/{id}/action/GetScanItemTemplateDefault")]
        public MScanItemTemplate GetScanItemTemplateDefault(string id)
        {
            var userName = "";
            var nameObj = Response.HttpContext.Items["Temp-Identity-Name"];
            if (nameObj != null)
            {
                userName = nameObj.ToString()!;;
            }

            var result = svc.GetScanItemTemplateDefault(id, userName);
            return result;
        }

        [HttpPost]
        [Route("org/{id}/action/AddScanItemTemplate")]
        public MVScanItemTemplate? AddScanItemTemplate(string id, [FromBody] MScanItemTemplate request)
        {
            var result = svc.AddScanItemTemplate(id, request);
            Response.Headers.Append("CUST_STATUS", result!.Status);

            return result;
        }

        [HttpPost]
        [Route("org/{id}/action/UpdateScanItemTemplateById/{actionId}")]
        public IActionResult UpdateScanItemTemplateById(string id, string actionId, [FromBody] MScanItemTemplate request)
        {
            var result = svc.UpdateScanItemTemplateById(id, actionId, request);
            Response.Headers.Append("CUST_STATUS", result!.Status);

            return Ok(result);
        }
    }
}
