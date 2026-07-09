using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.ViewsModels
{
    [ExcludeFromCodeCoverage]
    public class VMAuditLog : VMQueryBase
    {
        public string? FullTextSearch { get; set; }
        public List<string>? OrgIds { get; set; }
        public string? Interval { get; set; }
        public bool ReturnDocs { get; set; } = true;
        public string? ApplicationType { get; set; }
        public string? GroupBy { get; set; }
        public string? FilterApi { get; set; }
        public string? FilterUser { get; set; }
        public string? FilterIp { get; set; }
        public int? FilterStatus { get; set; }
    }
}
