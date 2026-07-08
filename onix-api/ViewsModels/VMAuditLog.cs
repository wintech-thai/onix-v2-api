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
    }
}
