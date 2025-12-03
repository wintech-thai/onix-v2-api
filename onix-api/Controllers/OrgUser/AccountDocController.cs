using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Models;
using Its.Onix.Api.Services;
using Its.Onix.Api.ViewsModels;

namespace Prom.LPR.Api.Controllers
{
    [ApiController]
    [Authorize(Policy = "GenericRolePolicy")]
    [Route("/api/[controller]")]
    public class AccountDocController : ControllerBase
    {
        private readonly IAccountDocService svc;

        [ExcludeFromCodeCoverage]
        public AccountDocController(IAccountDocService service)
        {
            svc = service;
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/GetAccountDocCashInvoices")]
        public async Task<IActionResult> GetAccountDocCashInvoices(string id, [FromBody] VMAccountDoc request)
        {
            request.DocumentType = "CashInvoice";
            var result = await svc.GetAccountDocs(id, request);

            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/GetAccountDocCashInvoiceCount")]
        public async Task<IActionResult> GetAccountDocCashInvoiceCount(string id, [FromBody] VMAccountDoc request)
        {
            request.DocumentType = "CashInvoice";
            var result = await svc.GetAccountDocCount(id, request);

            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/AddAccountDocCashInvoice")]
        public async Task<IActionResult> AddAccountDocCashInvoice(string id, [FromBody] MAccountDoc request)
        {
            request.DocumentType = "CashInvoice";
            var result = await svc.AddAccountDoc(id, request);

            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{id}/action/GetAccountDocById/{accountDocId}")]
        public async Task<IActionResult> GetAccountDocById(string id, string accountDocId)
        {
            var result = await svc.GetAccountDocById(id, accountDocId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/UpdateAccountDocById/{accountDocId}")]
        public async Task<IActionResult> UpdateAccountDocById(string id, string accountDocId, [FromBody] MAccountDoc request)
        {
            var result = await svc.UpdateAccountDocById(id, accountDocId, request);

            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/ApproveAccountDocById/{accountDocId}")]
        public async Task<IActionResult> ApproveAccountDocById(string id, string accountDocId)
        {
            var result = await svc.ApproveAccountDocById(id, accountDocId);

            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpDelete]
        [Route("org/{id}/action/DeleteAccountDocById/{accountDocId}")]
        public async Task<IActionResult> DeleteAccountDocById(string id, string accountDocId)
        {
            var result = await svc.DeleteAccountDocById(id, accountDocId);

            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/AddAccountDocItem/{accountDocId}")]
        public async Task<IActionResult> AddAccountDocItem(string id, string accountDocId, [FromBody] MAccountDocItem request)
        {
            request.AccountDocId = accountDocId;
            var result = await svc.AddAccountDocItem(id, accountDocId, request);

            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpDelete]
        [Route("org/{id}/action/DeleteAccountDocItemById/{accountDocItemId}")]
        public async Task<IActionResult> DeleteAccountDocItemById(string id, string accountDocItemId)
        {
            var result = await svc.DeleteAccountDocItemById(id, accountDocItemId);

            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/UpdateAccountDocItemById/{accountDocItemId}")]
        public async Task<IActionResult> UpdateAccountDocItemById(string id, string accountDocItemId, [FromBody] MAccountDocItem request)
        {
            request.AccountDocId = accountDocItemId;
            var result = await svc.UpdateAccountDocItemById(id, accountDocItemId, request);

            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{id}/action/GetAccountDocItemsById/{accountDocId}")]
        public async Task<IActionResult> GetAccountDocItemsById(string id, string accountDocId)
        {
            var result = await svc.GetAccountDocItemsById(id, accountDocId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/CalculateAccountDocItemPrice")]
        public async Task<IActionResult> CalculateAccountDocItemPrice(string id, [FromBody] MAccountDocItem request)
        {
            var result = await svc.CalculateAccountDocItemPrice(id, request);
            return Ok(result);
        }
    }
}
