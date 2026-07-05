using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.ViewsModels
{
    [ExcludeFromCodeCoverage]
    public class VMCaseManagement : VMQueryBase
    {
        public string? FullTextSearch { get; set; }
        public string? Status { get; set; }
        public string? Priority { get; set; }
        public string? OrgIdFilter { get; set; } // admin only
    }
}
