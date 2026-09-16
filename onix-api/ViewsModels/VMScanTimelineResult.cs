using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.ViewsModels
{
    [ExcludeFromCodeCoverage]
    public class VMScanTimelineBucket
    {
        public string? Timestamp { get; set; }
        public int Total { get; set; }
        public Dictionary<string, int> ProductCounts { get; set; } = new();
    }

    [ExcludeFromCodeCoverage]
    public class VMScanTimelineResult
    {
        public List<VMScanTimelineBucket> Buckets { get; set; } = new();
        public string? Interval { get; set; }
        public int Total { get; set; }
    }
}
