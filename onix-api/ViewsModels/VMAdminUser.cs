using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.ViewsModels
{
    [ExcludeFromCodeCoverage]
    public class VMAdminUser : VMQueryBase
    {
        public string? FullTextSearch { get; set; }
    }
}
