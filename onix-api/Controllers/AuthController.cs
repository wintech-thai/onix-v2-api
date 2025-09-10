using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Services;

namespace Its.Onix.Api.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService svc;

        [ExcludeFromCodeCoverage]
        public AuthController(IAuthService service)
        {
            svc = service;
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/temp/action/Login")]
        public IActionResult Login([FromBody] UserLogin request)
        {
            var result = svc.Login(request);
            Response.HttpContext.Items.Add("Temp-Identity-Name", request.UserName);
            
            if (result.Status != "Success")
            {
                return Unauthorized("Unauthorized, incorrect user or password!!!");
            }
            
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/temp/action/Refresh")]
        public IActionResult Refresh([FromBody] RefreshTokenRequest request)
        {
            var result = svc.RefreshToken(request.RefreshToken);
            Response.HttpContext.Items.Add("Temp-Identity-Name", result.UserName);

            if (result.Status != "Success")
            {
                return Unauthorized("Unauthorized, incorrect refresh token!!!");
            }

            return Ok(result);
        }
    }
}
