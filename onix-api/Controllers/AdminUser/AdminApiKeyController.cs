using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Models;
using Its.Onix.Api.Services;
using Its.Onix.Api.ViewsModels;
using Its.Onix.Api.ModelsViews;

namespace Its.Onix.Api.Controllers
{
    [Authorize(Policy = "GenericRolePolicy")]
    [ApiController]
    [Route("/admin-api/[controller]")]
    public class AdminApiKeyController : ControllerBase
    {
        private readonly IApiKeyService svc;

        [ExcludeFromCodeCoverage]
        public AdminApiKeyController(IApiKeyService service)
        {
            svc = service;
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        //Check if API Key found & not expire in the specific organization
        [Route("org/global/action/VerifyApiKey/{apiKey}")]
        public IActionResult VerifyApiKey(string apiKey)
        {
            var result = svc.VerifyApiKey("global", apiKey);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/AddApiKey")]
        public IActionResult AddApiKey([FromBody] MApiKey request)
        {
            var result = svc.AddApiKey("global", request);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpDelete]
        [Route("org/global/action/DeleteApiKeyById/{keyId}")]
        public IActionResult DeleteApiKeyById(string keyId)
        {
            var result = svc.DeleteApiKeyById("global", keyId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        // Use POST method, in the future we might send the body
        [HttpPost]
        [Route("org/global/action/GetApiKeys")]
        public IActionResult GetApiKeys([FromBody] VMApiKey param)
        {
            var result = svc.GetApiKeys("global", param);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/global/action/GetApiKeyCount")]
        public IActionResult GetApiKeyCount([FromBody] VMApiKey param)
        {
            var result = svc.GetApiKeyCount("global", param);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/UpdateApiKeyById/{keyId}")]
        public IActionResult UpdateApiKeyById(string keyId, [FromBody] MApiKey request)
        {
            var result = svc.UpdateApiKeyById("global", keyId, request);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/EnableApiKeyById/{keyId}")]
        public IActionResult EnableApiKeyById(string keyId)
        {
            var result = svc.UpdateApiKeyStatusById("global", keyId, "Active");
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/DisableApiKeyById/{keyId}")]
        public IActionResult DisableApiKeyById(string keyId)
        {
            var result = svc.UpdateApiKeyStatusById("global", keyId, "Disabled");
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/global/action/GetApiKeyById/{keyId}")]
        public async Task<MVApiKey> GetApiKeyById(string keyId)
        {
            var result = await svc.GetApiKeyById("global", keyId);
            return result;
        }
    }
}
