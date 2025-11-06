using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Services;
using Its.Onix.Api.ViewsModels;
using Its.Onix.Api.Models;

namespace Its.Onix.Api.Controllers
{
    [Authorize(Policy = "GenericRolePolicy")]
    [ApiController]
    [Route("/api/[controller]")]
    public class LimitController : ControllerBase
    {
        private readonly ILimitService svc;

        public LimitController(ILimitService service)
        {
            svc = service;
        }

        [HttpGet]
        [Route("org/{id}/action/GetLimits")]
        public async Task<IActionResult> GetLimits(string id)
        {
            var param = new VMLimit()
            {
                StatCode = ""
            };

            if (param.Limit <= 0)
            {
                param.Limit = 100;
            }

            var result = await svc.GetLimits(id, param);
            return Ok(result);
        }

        [HttpGet]
        [Route("org/{id}/action/GetLimitCount")]
        public async Task<IActionResult> GetLimitCount(string id)
        {
            var param = new VMLimit()
            {
                StatCode = ""
            };

            var result = await svc.GetLimitCount(id, param);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/{id}/action/UpdateLimit/{statCode}/{limit}")]
        public async Task<IActionResult> UpdateLimit(string id, string statCode, long limit)
        {
            //TODO : ควรย้าย API นี้ออกไปให้เฉพาะ admin เท่านั้น ไม่อย่างนั้น user ที่มีสิทธ์เป็น OWNER ก็จะเพิ่ม limit ได้เอง
            var lim = new MLimit()
            {
                Limit = limit,
                StatCode = statCode
            };

            var result = await svc.UpsertLimit(id, lim);
            Response.Headers.Append("CUST_STATUS", result!.Status);

            return Ok(result);
        }
    }
}
