using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.ModelsViews
{
    [ExcludeFromCodeCoverage]
    public class MVAgentEventTimeSeries
    {
        public string? Time { get; set; }
        public string? EventType { get; set; }
        public int Count { get; set; }
    }
}
