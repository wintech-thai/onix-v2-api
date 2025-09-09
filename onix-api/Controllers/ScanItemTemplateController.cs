using System.Diagnostics.CodeAnalysis;
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

        [ExcludeFromCodeCoverage]
        public ScanItemTemplateController(IScanItemTemplateService service)
        {
            svc = service;
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{id}/action/GetScanItemTemplate")]
        public MScanItemTemplate GetScanItemTemplate(string id)
        {
            var result = svc.GetScanItemTemplate(id);
            return result;
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/AddScanItemTemplate")]
        public MVScanItemTemplate? AddScanItemTemplate(string id, [FromBody] MScanItemTemplate request)
        {
            var result = svc.AddScanItemTemplate(id, request);
            Response.Headers.Append("CUST_STATUS", result!.Status);

            return result;
        }

        [ExcludeFromCodeCoverage]
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
