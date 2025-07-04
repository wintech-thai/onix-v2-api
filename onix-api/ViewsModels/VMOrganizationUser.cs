using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.ViewsModels
{
    [ExcludeFromCodeCoverage]
    public class VMOrganizationUser : VMQueryBase
    {
        public string? FullTextSearch { get; set; }
    }
}
