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
    public class ScanItemActionController : ControllerBase
    {
        private readonly IScanItemActionService svc;

        public ScanItemActionController(IScanItemActionService service)
        {
            svc = service;
        }

        [HttpGet]
        [Route("org/{id}/action/GetScanItemAction")]
        public MScanItemAction GetScanItemAction(string id)
        {
            var result = svc.GetScanItemAction(id);
            return result;
        }

        [HttpGet]
        [Route("org/{id}/action/GetScanItemActionDefault")]
        public MScanItemAction GetScanItemActionDefault(string id)
        {
            var result = svc.GetScanItemActionDefault(id);
            return result;
        }

        [HttpPost]
        [Route("org/{id}/action/AddScanItemAction")]
        public MVScanItemAction? AddScanItemAction(string id, [FromBody] MScanItemAction request)
        {
            var result = svc.AddScanItemAction(id, request);
            Response.Headers.Append("CUST_STATUS", result!.Status);

            return result;
        }

        [HttpPost]
        [Route("org/{id}/action/UpdateScanItemActionById/{actionId}")]
        public IActionResult UpdateScanItemActionById(string id, string actionId, [FromBody] MScanItemAction request)
        {
            var result = svc.UpdateScanItemActionById(id, actionId, request);
            Response.Headers.Append("CUST_STATUS", result!.Status);

            return Ok(result);
        }
    }
}
