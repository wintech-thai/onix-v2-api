using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.ViewsModels
{
    [ExcludeFromCodeCoverage]
    public class VMOrganization : VMQueryBase
    {
        public string? FullTextSearch { get; set; }
        public string? OrgType { get; set; }
        public string? Status { get; set; }
    }
}
