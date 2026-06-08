using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Services;
using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Controllers
{
    [Authorize(Policy = "GenericRolePolicy")]
    [ApiController]
    [Route("/admin-api/[controller]")]
    public class AdminNotiChannelController : ControllerBase
    {
        private readonly INotiChannelService svc;

        [ExcludeFromCodeCoverage]
        public AdminNotiChannelController(INotiChannelService service)
        {
            svc = service;
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/AddNotiChannel")]
        public async Task<IActionResult> AddNotiChannel([FromBody] MNotiChannel request)
        {
            var result = await svc.AddNotiChannel("global", request);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpDelete]
        [Route("org/global/action/DeleteNotiChannelById/{notiChannelId}")]
        public async Task<IActionResult> DeleteNotiChannelById(string notiChannelId)
        {
            var result = await svc.DeleteNotiChannelById("global", notiChannelId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/global/action/GetNotiChannelById/{notiChannelId}")]
        public async Task<IActionResult> GetNotiChannelById(string notiChannelId)
        {
            var result = await svc.GetNotiChannelById("global", notiChannelId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/UpdateNotiChannelById/{notiChannelId}")]
        public async Task<IActionResult> UpdateNotiChannelById(string notiChannelId, [FromBody] MNotiChannel request)
        {
            var result = await svc.UpdateNotiChannelById("global", notiChannelId, request);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/global/action/GetNotiChannels")]
        public async Task<IActionResult> GetNotiChannels([FromBody] VMNotiChannel request)
        {
            var result = await svc.GetNotiChannels("global", request);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/global/action/GetNotiEvents")]
        public IActionResult GetNotiEvents([FromBody] VMJob request)
        {
            var result = svc.GetNotiEvents("global", request);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/global/action/GetNotiEventCount")]
        public IActionResult GetNotiEventCount([FromBody] VMJob request)
        {
            var result = svc.GetNotiEventCount("global", request);
            return Ok(result);
        }

        [HttpGet]
        [Route("org/global/action/GetEventTypes")]
        public IActionResult GetEventTypes()
        {
            var result = svc.GetEventTypes("global");
            return Ok(result);
        }

        [HttpGet]
        [Route("org/global/action/GetChannelTypes")]
        public IActionResult GetChannelTypes()
        {
            var result = svc.GetChannelTypes("global");
            return Ok(result);
        }

        [HttpPost]
        [Route("org/global/action/GetNotiChannelCount")]
        public async Task<IActionResult> GetNotiChannelCount([FromBody] VMNotiChannel request)
        {
            var result = await svc.GetNotiChannelCount("global", request);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/EnableNotiChannelById/{notiChannelId}")]
        public async Task<IActionResult> EnableNotiChannelById(string notiChannelId)
        {
            var result = await svc.UpdateNotiChannelStatusById("global", notiChannelId, "Enabled");
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/DisableNotiChannelById/{notiChannelId}")]
        public async Task<IActionResult> DisableNotiChannelById(string notiChannelId)
        {
            var result = await svc.UpdateNotiChannelStatusById("global", notiChannelId, "Disabled");
            return Ok(result);
        }
    }
}
