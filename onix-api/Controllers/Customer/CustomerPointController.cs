using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Models;
using Its.Onix.Api.Services;

namespace Prom.LPR.Api.Controllers
{
    [ApiController]
    [Authorize(Policy = "GenericRolePolicy")]
    [Route("/customer-api/[controller]")]
    public class CustomerPointController : ControllerBase
    {
        private readonly IAdminUserService svc;

        [ExcludeFromCodeCoverage]
        public CustomerPointController(IAdminUserService service)
        {
            svc = service;
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{orgId}/action/GetWallets")]
        public IActionResult GetWallets(string orgId)
        {
            return Ok("");
        }
    }
}
