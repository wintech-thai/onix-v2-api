using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Services;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Controllers
{
    [Authorize(Policy = "GenericRolePolicy")]
    [ApiController]
    [Route("/api/[controller]")]
    public class StatController : ControllerBase
    {
        private readonly IStatService svc;

        public StatController(IStatService service)
        {
            svc = service;
        }

        [HttpPost]
        [Route("org/{id}/action/GetStats")]
        public IActionResult GetStats(string id, [FromBody] VMStat param)
        {
            if (param.Limit <= 0)
            {
                param.Limit = 100;
            }

            var result = svc.GetStats(id, param);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/{id}/action/GetStatCount")]
        public IActionResult GetStatCount(string id, [FromBody] VMStat param)
        {
            var result = svc.GetStatCount(id, param);
            return Ok(result);
        }
    }
}
