using Serilog;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Services;
using Its.Onix.Api.ModelsViews;
using System.Text.Json;
using Its.Onix.Api.Utils;
using System.Web;
using Its.Onix.Api.Models;

namespace Its.Onix.Api.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class RegistrationController : ControllerBase
    {
        private readonly IScanItemService svc;
        private readonly IRedisHelper _redis;
        private readonly IAuthService _authService;

        public RegistrationController(IScanItemService service,
            IAuthService authService,
            IRedisHelper redis)
        {
            svc = service;
            _redis = redis;
            _authService = authService;
        }

        [HttpPost]
        [Route("org/{id}/action/ConfirmExistingUserInvitation/{token}/{userName}")]
        public IActionResult ConfirmExistingUserInvitation(string id, string token, string usrName, [FromBody] MUserRegister request)
        {
            return Ok("");
        }

        [HttpPost]
        [Route("org/{id}/action/ConfirmNewUserInvitation/{token}/{userName}")]
        public IActionResult ConfirmNewUserInvitation(string id, string token, string usrName, [FromBody] MUserRegister request)
        {
            return Ok("");
        }
    }
}
