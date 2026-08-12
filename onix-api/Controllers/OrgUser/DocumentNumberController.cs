using System.Diagnostics.CodeAnalysis;
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
    public class DocumentNumberController : ControllerBase
    {
        private readonly IDocumentNumberService _svc;

        [ExcludeFromCodeCoverage]
        public DocumentNumberController(IDocumentNumberService service)
        {
            _svc = service;
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/AddDocumentNumberConfig")]
        public async Task<IActionResult> AddDocumentNumberConfig(string id, [FromBody] MDocumentNumberConfig request)
        {
            try
            {
                var result = await _svc.AddDocumentNumberConfig(id, request);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { status = "ERROR_DUPLICATE_DOCUMENT_TYPE", description = ex.Message });
            }
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{id}/action/GetDocumentNumberConfigById/{configId}")]
        public async Task<IActionResult> GetDocumentNumberConfigById(string id, string configId)
        {
            var result = await _svc.GetDocumentNumberConfigById(id, configId);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/GetDocumentNumberConfigs")]
        public async Task<IActionResult> GetDocumentNumberConfigs(string id, [FromBody] VMDocumentNumberConfig param)
        {
            if (param.Limit <= 0) param.Limit = 100;
            var result = await _svc.GetDocumentNumberConfigs(id, param);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/GetDocumentNumberConfigCount")]
        public async Task<IActionResult> GetDocumentNumberConfigCount(string id, [FromBody] VMDocumentNumberConfig param)
        {
            var result = await _svc.GetDocumentNumberConfigCount(id, param);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/UpdateDocumentNumberConfigById/{configId}")]
        public async Task<IActionResult> UpdateDocumentNumberConfigById(string id, string configId, [FromBody] MDocumentNumberConfig request)
        {
            var result = await _svc.UpdateDocumentNumberConfigById(id, configId, request);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpDelete]
        [Route("org/{id}/action/DeleteDocumentNumberConfigById/{configId}")]
        public async Task<IActionResult> DeleteDocumentNumberConfigById(string id, string configId)
        {
            var result = await _svc.DeleteDocumentNumberConfigById(id, configId);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{id}/action/GetDocumentNumber/{docType}")]
        public async Task<IActionResult> GetDocumentNumber(string id, string docType)
        {
            var docNo = await _svc.GetDocumentNumber(id, docType);
            return Ok(new { DocumentNumber = docNo });
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/UpdateDocumentNumberSequence/{configId}")]
        public async Task<IActionResult> UpdateDocumentNumberSequence(string id, string configId, [FromBody] VMDocumentNumberSequence request)
        {
            var result = await _svc.UpdateDocumentNumberSequence(id, configId, request.CurrentSequenceNo, request.CurrentSequenceKey ?? "");
            if (result == null) return NotFound();
            return Ok(result);
        }
    }
}
