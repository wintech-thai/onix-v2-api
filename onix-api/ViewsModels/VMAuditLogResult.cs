using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Its.Onix.Api.ViewsModels
{
    [ExcludeFromCodeCoverage]
    public class VMAuditLogQueryResult
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = "OK";

        [JsonPropertyName("data")]
        public List<Dictionary<string, object?>> Data { get; set; } = new();

        [JsonPropertyName("total")]
        public int Total { get; set; }

        [JsonPropertyName("aggregations")]
        public VMAuditLogAggregations? Aggregations { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class VMAuditLogAggregations
    {
        [JsonPropertyName("timeline")]
        public VMAggBuckets Timeline { get; set; } = new();

        [JsonPropertyName("by_api")]
        public VMAggBuckets ByApi { get; set; } = new();

        [JsonPropertyName("by_user")]
        public VMAggBuckets ByUser { get; set; } = new();

        [JsonPropertyName("by_ip")]
        public VMAggBuckets ByIp { get; set; } = new();

        [JsonPropertyName("by_status")]
        public VMAggBuckets ByStatus { get; set; } = new();

        [JsonPropertyName("bruteforce")]
        public VMBruteforceAgg Bruteforce { get; set; } = new();
    }

    [ExcludeFromCodeCoverage]
    public class VMBruteforceAgg
    {
        [JsonPropertyName("by_ip")]
        public VMAggBuckets ByIp { get; set; } = new();
    }

    [ExcludeFromCodeCoverage]
    public class VMAggBuckets
    {
        [JsonPropertyName("buckets")]
        public List<VMAggBucket> Buckets { get; set; } = new();
    }

    [ExcludeFromCodeCoverage]
    public class VMAggBucket
    {
        [JsonPropertyName("key")]
        public object Key { get; set; } = "";

        [JsonPropertyName("key_as_string")]
        public string? KeyAsString { get; set; }

        [JsonPropertyName("doc_count")]
        public int DocCount { get; set; }
    }
}
