using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Services;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Controllers
{
    [ApiController]
    [Authorize(Policy = "GenericRolePolicy")]
    [Route("/api/[controller]")]
    public class PointTopUpController : ControllerBase
    {
        private readonly IPointTopUpService svc;

        [ExcludeFromCodeCoverage]
        public PointTopUpController(IPointTopUpService service)
        {
            svc = service;
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/CreatePointTopUp")]
        public async Task<IActionResult> CreatePointTopUp(string id, [FromBody] VMPointTopUp request)
        {
            var result = await svc.CreatePointTopUp(id, request.Amount);
            return Ok(result);
        }
    }
}
