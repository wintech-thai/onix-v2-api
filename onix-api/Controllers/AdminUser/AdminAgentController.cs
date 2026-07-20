using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Services;
using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;
using System.Text.Json;
using Its.Onix.Api.ModelsViews;
using System.Text.RegularExpressions;
using QRCoder;

namespace Its.Onix.Api.Controllers
{
    [Authorize(Policy = "GenericRolePolicy")]
    [ApiController]
    [Route("/admin-api/[controller]")]
    public class AdminAgentController : ControllerBase
    {
        private readonly IAgentService svc;
        private readonly IApiKeyService _apiKeySvc;
        private readonly IPaymentTransactionService _paymentTxSvc;

        [ExcludeFromCodeCoverage]
        public AdminAgentController(IAgentService service, IApiKeyService apiKeySvc, IPaymentTransactionService paymentTxSvc)
        {
            svc = service;
            _apiKeySvc = apiKeySvc;
            _paymentTxSvc = paymentTxSvc;
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
        [Route("org/global/action/AddLineApiAgent")]
        public async Task<IActionResult> AddLineApiAgent([FromBody] MAgent request)
        {
            var result = await svc.AddLineApiAgent("global", request);
            return Ok(result);
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
        [Route("org/global/action/GetAgentEventTimeSeries/{agentId}")]
        public async Task<IActionResult> GetAgentEventTimeSeries(string agentId, [FromBody] VMAgentEvent request)
        {
            request.AgentId = agentId;
            var result = await svc.GetAgentEventTimeSeries("global", request);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/global/action/NotifyHeartbeat/{agentId}")]
        public async Task<IActionResult> NotifyHeartbeat(string agentId, Dictionary<string, object> body)
        {
            var eventJson = JsonSerializer.Serialize(body);
            var metaData = string.Join(",", GetMetaData(body));
            var errorCnt = GetErrorCount(body);

            var evt = new MAgentEvent()
            {
                AgentId = agentId,
                EventType = "Heartbeat",
                RawData = eventJson,
                Channel = "APP",
                Tags = metaData,
                ErrorCount = errorCnt,
                Status = "OK",
                StatusDesc = "Success",
            };

            var result = await svc.AddAgentEvent("global", evt);
            return Ok(result);
        }

        private static List<string?> GetMetaData(Dictionary<string, object> body)
        {
            var bd = body;
            if (body.TryGetValue("rawDataObj", out object? value) && (value is JsonElement element))
            {
                //ตรงนี้เป็นของเดิม ที่ตัว android app ส่งข้อมูลมาผิดอยู่
                //bd = (Dictionary<string, object>) value;
                bd = JsonSerializer.Deserialize<Dictionary<string, object>>(element)!;
            }

            //Line
            bd.TryGetValue("title", out var titleObj);
            bd.TryGetValue("sourceLabel", out var sourceLabelObj);

            //Heartbeat
            bd.TryGetValue("AppVersion", out var appVersionObj);
            bd.TryGetValue("Model", out var modelObj);

            var title = titleObj?.ToString();
            var sourceLabel = sourceLabelObj?.ToString();
            var appVersion = appVersionObj?.ToString();
            var model = modelObj?.ToString();

            List<string?> arr = [title, sourceLabel, appVersion, model];
            arr = [.. arr.Where(x => !string.IsNullOrWhiteSpace(x))];

            return arr;
        }


        private static int GetErrorCount(Dictionary<string, object> body)
        {
            var bd = body;

            //Heartbeat
            if (!bd.TryGetValue("crashes", out var errorObjArr) || errorObjArr == null)
            {
                return 0;
            }

            var errorCnt = errorObjArr switch
            {
                List<object> arr => arr.Count,
                object[] arr => arr.Length,
                JsonElement json when json.ValueKind == JsonValueKind.Array
                    => json.GetArrayLength(),
                _ => 0
            };

            return errorCnt;
        }

        private string GetChannel(Dictionary<string, object> body)
        {
            var bd = body;
            if (body.TryGetValue("rawDataObj", out object? value) && (value is JsonElement element))
            {
                //ตรงนี้เป็นของเดิม ที่ตัว android app ส่งข้อมูลมาผิดอยู่
                //bd = (Dictionary<string, object>) value;
                bd = JsonSerializer.Deserialize<Dictionary<string, object>>(element)!;
            }

            bd.TryGetValue("sourceLabel", out var sourceLabelObj);
            var sourceLabel = sourceLabelObj?.ToString();

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
            var bd = body;
            if (body.TryGetValue("rawDataObj", out object? value) && (value is JsonElement element))
            {
                //ตรงนี้เป็นของเดิม ที่ตัว android app ส่งข้อมูลมาผิดอยู่
                //bd = (Dictionary<string, object>) value;
                bd = JsonSerializer.Deserialize<Dictionary<string, object>>(element)!;
            }

            var pmt = new MPaymentNotiLine()
            {
                TxType = "PayIn",
                RemainAmount = 0,
                SourceBankAccountNo = "",
                MerchantId = null,
                OriginalData = body,
            };

            pmt.OriginalData.Add("sourceApi", "AdminAgentController.NotifyLineMessage");

            if (channel == "LINE")
            {
                var title = bd["title"].ToString();
                var text = bd["text"].ToString();

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
                else if ((title == "SCB Connect") && !string.IsNullOrEmpty(text))
                {
                    pmt.DestinationBankCode = "SCB";

                    var match = Regex.Match(
                        text,
                        @"รายการเงินเข้า\s*(?<amount>[\d,]+\.\d{2})\s*บาท\s*เข้าบัญชี\s*(?<account>[A-Z0-9-]+)"
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

        private async Task<MVBankAccount> GetBankAccount(MPaymentNotiLine lineNoti, string agentId)
        {
            var r = new MVBankAccount()
            {
                Status = "OK",
                Description = "Success",
            };

            var bankCode = lineNoti.DestinationBankCode;
            var mvAgent = await svc.GetAgentById("global", agentId);
            var agent = mvAgent.Agent!;

            var bankAccounts = agent.BankAccountsSelectedObj;
            MBankAccount? selectedBankAccount = null;

            foreach (var ba in bankAccounts)
            {
                if (ba.BankCode == bankCode)
                {
                    selectedBankAccount = ba;
                    break;
                }
            }

            if (selectedBankAccount == null)
            {
                //หาตัว match ไม่เจอที่ ธนาคารเดียวกัน
                r.Status = "BANK_NOT_FOUND";
                r.Description = $"Unable to find bank account with bank code [{bankCode}]";
                return r;
            }

            r.BankAccount = selectedBankAccount;
            return r;
        }

        [HttpPost]
        [Route("org/global/action/NotifyLineMessage/{agentId}")]
        public async Task<IActionResult> NotifyLineMessage(string agentId, Dictionary<string, object> body)
        {
            Dictionary<string, object> wrapData = [];

            var metaData = string.Join(",", GetMetaData(body));
            var channel = GetChannel(body);

            var pmtLineNoti = GetPaymentNoti(body, channel);
            var mvBa = await GetBankAccount(pmtLineNoti!, agentId);
            var ba = mvBa.BankAccount;

            wrapData.Add("InputData", body);
            wrapData.Add("PaymentNoti", pmtLineNoti!);
            wrapData.Add("BankAccount", mvBa.BankAccount!);

            var eventJson = JsonSerializer.Serialize(wrapData);

            var evt = new MAgentEvent()
            {
                AgentId = agentId,
                EventType = "PaymentTx",
                RawData = eventJson,
                Tags = metaData,
                Channel = channel,
                ErrorCount = 0,
                //PaymentNoti = pmtLineNoti,
                //BankAccount = mvBa.BankAccount!,

                Status = mvBa.Status,
                StatusDesc = mvBa.Description,
            };

            if (ba != null)
            {
                var bankAccountId = ba.Id.ToString()!;
                var mvTx = await _paymentTxSvc.ProcessLinePaymentTxNotification("global", bankAccountId, pmtLineNoti!);

                if (mvTx.Status != "OK")
                {
                    evt.Status = mvTx.Status;
                    evt.StatusDesc = mvTx.Description;
                }
            }
            
            var result = await svc.AddAgentEvent("global", evt);

            return Ok(result);
        }
    }
}
