using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.ViewsModels
{
    [ExcludeFromCodeCoverage]
    public class VMAgent : VMQueryBase
    {
        public string? FullTextSearch { get; set; }
    }
}
