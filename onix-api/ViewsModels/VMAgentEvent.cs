using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.ViewsModels
{
    [ExcludeFromCodeCoverage]
    public class VMAgentEvent : VMQueryBase
    {
        public string? FullTextSearch { get; set; }
        public string? EventType { get; set; }
        public string? Channel { get; set; }
        public string? AgentId { get; set; }
    }
}
