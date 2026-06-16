using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Services;
using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;
using System.Text.Json;
using Its.Onix.Api.ModelsViews;
using System.Text.RegularExpressions;

namespace Its.Onix.Api.Controllers
{
    [Authorize(Policy = "GenericRolePolicy")]
    [ApiController]
    [Route("/admin-api/[controller]")]
    public class AdminAgentController : ControllerBase
    {
        private readonly IAgentService svc;
        private readonly IApiKeyService _apiKeySvc;

        [ExcludeFromCodeCoverage]
        public AdminAgentController(IAgentService service, IApiKeyService apiKeySvc)
        {
            svc = service;
            _apiKeySvc = apiKeySvc;
        }


        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/global/action/GetAgentEndPoints/{agentId}")]
        public async Task<IActionResult> GetAgentEndPoints(string agentId)
        {
            var mvAgent = await svc.GetAgentById("global", agentId);
            if (mvAgent.Status != "OK")
            {
                return Ok(mvAgent);
            }

            var mc = mvAgent.Agent!;

            var agentOrgId = mc.OrgId;
            var url1 = $"https://<PAYMENT-REQUEST-SERVICE>/admin-api/AdminAgent/org/{agentOrgId}/action/NotifyHeartbeat/{agentId}";
            var url2 = $"https://<PAYMENT-REQUEST-SERVICE>/admin-api/AdminAgent/org/{agentOrgId}/action/NotifyLineMessage/{agentId}";

            var result = new MVEndPoint()
            {
                Status = "OK",
                Description = "Success",
                AgentHeartbeatUrl = url1,
                PaymentTxNotiUrl = url2,
            };

            return Ok(result);
        }


        [HttpPost]
        [Route("org/global/action/CreateAgentApiKey/{agentId}")]
        public IActionResult CreateAgentApiKey(string agentId)
        {
            var uuid = Guid.NewGuid();

            var request = new MApiKey()
            {
                KeyType = $"Agent:{agentId}",
                KeyName = $"Agent:{uuid}",
                KeyDescription = "Auto generated key, DO NOT delete!!!",
                Roles = [ "AGENT_CONNECT" ], 
            };

            var apiKey = _apiKeySvc.AddApiKey("global", request);
            return Ok(apiKey);
        }

        [HttpGet]
        [Route("org/global/action/GetAgentApiKeys/{agentId}")]
        public IActionResult GetAgentApiKeys(string agentId)
        {
            var request = new VMApiKey()
            {
                KeyType = $"Agent:{agentId}", 
            };

            var keys = _apiKeySvc.GetApiKeys("global", request);

            return Ok(keys);
        }


        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/AddAgent")]
        public async Task<IActionResult> AddAgent([FromBody] MAgent request)
        {
            var result = await svc.AddAgentSimple("global", request);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpDelete]
        [Route("org/global/action/DeleteAgentById/{agentId}")]
        public async Task<IActionResult> DeleteAgentById(string agentId)
        {
            var result = await svc.DeleteAgentById("global", agentId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/global/action/GetAgentById/{agentId}")]
        public async Task<IActionResult> GetAgentById(string agentId)
        {
            var result = await svc.GetAgentById("global", agentId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/global/action/UpdateAgentById/{agentId}")]
        public async Task<IActionResult> UpdateAgentById(string agentId, [FromBody] MAgent request)
        {
            var result = await svc.UpdateAgentById("global", agentId, request);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/global/action/GetAgents")]
        public async Task<IActionResult> GetAgents([FromBody] VMAgent request)
        {
            var result = await svc.GetAgents("global", request);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/global/action/GetAgentCount")]
        public async Task<IActionResult> GetAgentCount([FromBody] VMAgent request)
        {
            var result = await svc.GetAgentCount("global", request);
            return Ok(result);
        }

        [HttpGet]
        [Route("org/global/action/GetAgentEventById/{agentEventId}")]
        public async Task<IActionResult> GetAgentEvents(string agentEventId)
        {
            var result = await svc.GetAgentEventById("global", agentEventId);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/global/action/GetAgentEvents/{agentId}")]
        public async Task<IActionResult> GetAgentEvents(string agentId, [FromBody] VMAgentEvent request)
        {
            request.AgentId = agentId;
            var result = await svc.GetAgentEvents("global", request);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/global/action/GetAgentEventCount/{agentId}")]
        public async Task<IActionResult> GetAgentEventCount(string agentId, [FromBody] VMAgentEvent request)
        {
            request.AgentId = agentId;
            var result = await svc.GetAgentEventCount("global", request);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/global/action/NotifyHeartbeat/{agentId}")]
        public async Task<IActionResult> NotifyHeartbeat(string agentId, Dictionary<string, object> body)
        {
            var eventJson = JsonSerializer.Serialize(body);
            var metaData = string.Join(",", GetMetaData(body));

            var evt = new MAgentEvent()
            {
                AgentId = agentId,
                EventType = "Heartbeat",
                RawData = eventJson,
                Channel = "APP",
                Tags = metaData,
            };

            var result = await svc.AddAgentEvent("global", evt);
            return Ok(result);
        }

        private static List<string?> GetMetaData(Dictionary<string, object> body)
        {
            var title = body["title"].ToString();
            var sourceLabel = body["sourceLabel"]?.ToString();

            return [ title, sourceLabel];
        }

        private string GetChannel(Dictionary<string, object> body)
        {
            var sourceLabel = body["sourceLabel"].ToString();
            if (!string.IsNullOrEmpty(sourceLabel))
            {
                //เฉพาะ LINE จะมี field นี้
                return sourceLabel;
            }

            //TODO : Check เพิ่มเติมถ้าเป็น SMS

            return "SMS";
        }

        private static MPaymentNotiLine? GetPaymentNoti(Dictionary<string, object> body, string channel)
        {
            var pmt = new MPaymentNotiLine()
            {
                TxType = "PayIn",
                RemainAmount = 0,
                SourceBankAccountNo = "",
                MerchantId = null,
            };

            if (channel == "LINE")
            {
                var title = body["title"].ToString();
                var text = body["text"].ToString();

                if ((title == "Krungthai Connext") && !string.IsNullOrEmpty(text))
                {
                    pmt.DestinationBankCode = "KTB";

                    var match = Regex.Match(
                        text,
                        @"เงินเข้า:\s*(?<amount>[\d,]+\.\d{2})\s*บาท\s*เข้าบัญชี\s*(?<account>[A-Z0-9]+)"
                    );

                    if (match.Success)
                    {
                        var amount = decimal.Parse(match.Groups["amount"].Value);
                        var account = match.Groups["account"].Value;

                        pmt.PaymentAmount = amount;
                        pmt.DestinationAccountNo = account;
                    }
                }
            }

            return pmt;
        }

        [HttpPost]
        [Route("org/global/action/NotifyLineMessage/{agentId}")]
        public async Task<IActionResult> NotifyLineMessage(string agentId, Dictionary<string, object> body)
        {
            Dictionary<string, object> wrapData = [];

            var metaData = string.Join(",", GetMetaData(body));
            var channel = GetChannel(body);
            var pmtLineNoti = GetPaymentNoti(body, channel);

            wrapData.Add("InputData", body);
            wrapData.Add("PaymentNoti", pmtLineNoti!);

            var eventJson = JsonSerializer.Serialize(wrapData);

            var evt = new MAgentEvent()
            {
                AgentId = agentId,
                EventType = "PaymentTx",
                RawData = eventJson,
                Tags = metaData,
                Channel = channel,
                PaymentNoti = pmtLineNoti,
            };

            var result = await svc.AddAgentEvent("global", evt);

            return Ok(result);
        }
    }
}
