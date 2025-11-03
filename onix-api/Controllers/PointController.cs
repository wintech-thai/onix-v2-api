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
    }
}
