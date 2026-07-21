
namespace Its.Onix.Api.Models
{
    public class MAgentConfig
    {
        public string? UserName { get; set; }
        public string? ApiKey { get; set; }
        public string? AgentImageTag { get; set; }
        public string? AgentImageRepo { get; set; }

        public MAgentConfig()
        {
        }
    }
}
