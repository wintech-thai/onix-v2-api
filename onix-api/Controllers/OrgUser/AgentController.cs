using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Services;
using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;
using System.Text.Json;
using Its.Onix.Api.ModelsViews;

namespace Its.Onix.Api.Controllers
{
    [Authorize(Policy = "GenericRolePolicy")]
    [ApiController]
    [Route("/api/[controller]")]
    public class AgentController : ControllerBase
    {
        private readonly IAgentService svc;

        [ExcludeFromCodeCoverage]
        public AgentController(IAgentService service)
        {
            svc = service;
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/AddAgent")]
        public async Task<IActionResult> AddAgent(string id, [FromBody] MAgent request)
        {
            var result = await svc.AddAgent(id, request);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpDelete]
        [Route("org/{id}/action/DeleteAgentById/{agentId}")]
        public async Task<IActionResult> DeleteAgentById(string id, string agentId)
        {
            var result = await svc.DeleteAgentById(id, agentId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{id}/action/GetAgentById/{agentId}")]
        public async Task<IActionResult> GetAgentById(string id, string agentId)
        {
            var result = await svc.GetAgentById(id, agentId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/UpdateAgentById/{agentId}")]
        public async Task<IActionResult> UpdateAgentById(string id, string agentId, [FromBody] MAgent request)
        {
            var result = await svc.UpdateAgentById(id, agentId, request);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/{id}/action/GetAgents")]
        public async Task<IActionResult> GetAgents(string id, [FromBody] VMAgent request)
        {
            var result = await svc.GetAgents(id, request);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/{id}/action/GetAgentCount")]
        public async Task<IActionResult> GetAgentCount(string id, [FromBody] VMAgent request)
        {
            var result = await svc.GetAgentCount(id, request);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/{id}/action/Heartbeat/{agentId}")]
        public async Task<IActionResult> Heartbeat(string id, string agentId, [FromBody] JsonElement body)
        {
            var result = await svc.AddAgentStat(id, agentId, body);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/{id}/action/EnableLineApiAgentById/{agentId}")]
        public async Task<IActionResult> EnableLineApiAgentById(string id, string agentId)
        {
            var result = await svc.EnableLineApiAgentById(id, agentId);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/{id}/action/DisableLineApiAgentById/{agentId}")]
        public async Task<IActionResult> DisableLineApiAgentById(string id, string agentId)
        {
            var result = await svc.DisableLineApiAgentById(id, agentId);
            return Ok(result);
        }

        // Merchant ไม่ได้มี orgId == "global" ดังนั้น repository จะ filter เป็นของ org ตัวเองเท่านั้นอัตโนมัติ —
        // ถ้า agent ไม่ใช่ของ org นี้จริง ๆ GetAgentById จะคืน NOTFOUND ให้เอง ก่อนจะยอม proxy ไปที่ pod
        [HttpGet]
        [Route("org/{id}/action/GetLineApiAgentStatus/{agentId}")]
        public async Task<IActionResult> GetLineApiAgentStatus(string id, string agentId)
        {
            var mvAgent = await svc.GetAgentById(id, agentId);
            if (mvAgent.Status != "OK" || mvAgent.Agent == null)
            {
                return Ok(new { ok = false, podStatus = "Offline", agentId, raw = (string?)null });
            }

            var baseUrl = GetLineAgentBaseUrl(agentId);
            try
            {
                using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
                http.DefaultRequestHeaders.Authorization = GetLineAgentAuth(agentId);
                var response = await http.GetAsync($"{baseUrl}/status");
                var body = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                    return Ok(new { ok = false, podStatus = "Offline", agentId, raw = body });

                var doc = JsonDocument.Parse(body);
                var ok = doc.RootElement.TryGetProperty("ok", out var okEl) && okEl.GetBoolean();
                string? login = null;
                if (doc.RootElement.TryGetProperty("login", out var loginEl))
                {
                    if (loginEl.ValueKind == JsonValueKind.String)
                        login = loginEl.GetString();
                    else if (loginEl.ValueKind == JsonValueKind.Object && loginEl.TryGetProperty("state", out var stateEl))
                        login = stateEl.GetString();
                }
                return Ok(new { ok, podStatus = ok ? "Running" : "Offline", login, agentId, raw = body });
            }
            catch
            {
                return Ok(new { ok = false, podStatus = "Offline", agentId, raw = (string?)null });
            }
        }

        [HttpGet]
        [Route("org/{id}/action/GetLineApiAgentLoginQR/{agentId}")]
        public async Task<IActionResult> GetLineApiAgentLoginQR(string id, string agentId)
        {
            var mvAgent = await svc.GetAgentById(id, agentId);
            if (mvAgent.Status != "OK" || mvAgent.Agent == null)
            {
                return Ok(new { ok = false, error = "Agent not found" });
            }

            var baseUrl = GetLineAgentBaseUrl(agentId);
            try
            {
                using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
                http.DefaultRequestHeaders.Authorization = GetLineAgentAuth(agentId);
                var response = await http.PostAsync($"{baseUrl}/login/qr", null);
                var body = await response.Content.ReadAsStringAsync();
                return Content(body, "application/json");
            }
            catch (Exception ex)
            {
                return Ok(new { ok = false, error = ex.Message });
            }
        }

        [HttpGet]
        [Route("org/{id}/action/GetLineApiAgentLoginStatus/{agentId}")]
        public async Task<IActionResult> GetLineApiAgentLoginStatus(string id, string agentId)
        {
            var mvAgent = await svc.GetAgentById(id, agentId);
            if (mvAgent.Status != "OK" || mvAgent.Agent == null)
            {
                return Ok(new { ok = false, error = "Agent not found" });
            }

            var baseUrl = GetLineAgentBaseUrl(agentId);
            try
            {
                using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
                http.DefaultRequestHeaders.Authorization = GetLineAgentAuth(agentId);
                var response = await http.GetAsync($"{baseUrl}/login/status");
                var body = await response.Content.ReadAsStringAsync();
                return Content(body, "application/json");
            }
            catch (Exception ex)
            {
                return Ok(new { ok = false, error = ex.Message });
            }
        }

        private static string GetLineAgentBaseUrl(string agentId)
        {
            var clean = agentId.Replace("-", "");
            var prefix = clean.Length >= 8 ? clean[..8].ToLower() : clean.ToLower();
            return $"http://line-agent-{prefix}";
        }

        private static System.Net.Http.Headers.AuthenticationHeaderValue GetLineAgentAuth(string agentId)
        {
            var credentials = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"api:{agentId}"));
            return new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);
        }

        [HttpPost]
        [Route("org/{id}/action/GetAgentEvents/{agentId}")]
        public async Task<IActionResult> GetAgentEvents(string id, string agentId, [FromBody] VMAgentEvent request)
        {
            request.AgentId = agentId;
            var result = await svc.GetAgentEvents(id, request);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/{id}/action/GetAgentEventCount/{agentId}")]
        public async Task<IActionResult> GetAgentEventCount(string id, string agentId, [FromBody] VMAgentEvent request)
        {
            request.AgentId = agentId;
            var result = await svc.GetAgentEventCount(id, request);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/{id}/action/GetAgentEventTimeSeries/{agentId}")]
        public async Task<IActionResult> GetAgentEventTimeSeries(string id, string agentId, [FromBody] VMAgentEvent request)
        {
            request.AgentId = agentId;
            var result = await svc.GetAgentEventTimeSeries(id, request);
            return Ok(result);
        }
    }
}
