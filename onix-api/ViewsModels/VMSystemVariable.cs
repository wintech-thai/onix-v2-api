using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.ViewsModels
{
    [ExcludeFromCodeCoverage]
    public class VMSystemVariable : VMQueryBase
    {
        public string? FullTextSearch { get; set; }
    }
}
