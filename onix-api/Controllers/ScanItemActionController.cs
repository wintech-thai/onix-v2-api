using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Models;
using Its.Onix.Api.Services;
using Its.Onix.Api.ViewsModels;
using Its.Onix.Api.ModelsViews;
using System.Text.Json;

namespace Its.Onix.Api.Controllers
{
    [Authorize(Policy = "GenericRolePolicy")]
    [ApiController]
    [Route("/api/[controller]")]
    public class ScanItemActionController : ControllerBase
    {
        private readonly IScanItemActionService svc;

        [ExcludeFromCodeCoverage]
        public ScanItemActionController(IScanItemActionService service)
        {
            svc = service;
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{id}/action/GetScanItemAction")]
        public MScanItemAction GetScanItemAction(string id)
        {
            var result = svc.GetScanItemAction(id);
            return result;
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/AddScanItemAction")]
        public MVScanItemAction? AddScanItemAction(string id, [FromBody] MScanItemAction request)
        {
            var result = svc.AddScanItemAction(id, request);
            Response.Headers.Append("CUST_STATUS", result!.Status);

            return result;
        }

        [ExcludeFromCodeCoverage]
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
