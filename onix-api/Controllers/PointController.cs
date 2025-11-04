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
    public class PointController : ControllerBase
    {
        private readonly IPointService svc;

        [ExcludeFromCodeCoverage]
        public PointController(IPointService service)
        {
            svc = service;
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/AddPoint/{walletId}")]
        public async Task<IActionResult> AddPoint(string id, string walletId, [FromBody] MPointTx request)
        {
            request.WalletId = walletId;
            var result = await svc.AddPoint(id, request);

            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/DeductPoint/{walletId}")]
        public async Task<IActionResult> DeductPoint(string id, string walletId, [FromBody] MPointTx request)
        {
            request.WalletId = walletId;
            var result = await svc.DeductPoint(id, request);

            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/GetPointTxsByWalletId/{walletId}")]
        public async Task<IActionResult> GetPointTxsByWalletId(string id, string walletId, [FromBody] VMPointTx request)
        {
            request.WalletId = walletId;
            var result = await svc.GetPointTxsByWalletId(id, request);

            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/GetPointTxsCountByWalletId/{walletId}")]
        public async Task<IActionResult> GetPointTxsCountByWalletId(string id, string walletId, [FromBody] VMPointTx request)
        {
            request.WalletId = walletId;
            var result = await svc.GetPointTxsCountByWalletId(id, request);

            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/GetPointBalanceByWalletId/{walletId}")]
        public async Task<IActionResult> GetPointBalanceByWalletId(string id, string walletId, [FromBody] VMPointBalance request)
        {
            request.WalletId = walletId;
            var result = await svc.GetPointBalanceByWalletId(id, request);

            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/AddWallet")]
        public async Task<IActionResult> AddWallet(string id, [FromBody] MWallet request)
        {
            var result = await svc.AddWallet(id, request);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/GetWallets")]
        public async Task<IActionResult> GetWallets(string id, [FromBody] VMWallet request)
        {
            var result = await svc.GetWallets(id, request);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/GetWalletsCount")]
        public async Task<IActionResult> GetWalletsCount(string id, [FromBody] VMWallet request)
        {
            var result = await svc.GetWalletsCount(id, request);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{id}/action/GetWalletById/{walletId}")]
        public async Task<IActionResult> GetWalletById(string id, string walletId)
        {
            var result = await svc.GetWalletById(id, walletId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/UpdateWalletById/{walletId}")]
        public async Task<IActionResult> UpdateWalletById(string id, string walletId, [FromBody] MWallet request)
        {
            var result = await svc.UpdateWalletById(id, walletId, request);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/AttachCustomerToWalletById/{walletId}/{customerId}")]
        public async Task<IActionResult> AttachCustomerToWalletById(string id, string walletId, string customerId)
        {
            var result = await svc.AttachCustomerToWalletById(id, walletId, customerId);
            return Ok(result);
        }
        
        [ExcludeFromCodeCoverage]
        [HttpDelete]
        [Route("org/{id}/action/DeleteWalletById/{walletId}")]
        public async Task<IActionResult> DeleteWalletById(string id, string walletId)
        {
            var result = await svc.DeleteWalletById(id, walletId);
            return Ok(result);
        }
    }
}
