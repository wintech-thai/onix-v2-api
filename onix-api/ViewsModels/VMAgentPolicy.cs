using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.ViewsModels
{
    [ExcludeFromCodeCoverage]
    public class VMAgentPolicy : VMQueryBase
    {
        public string? FullTextSearch { get; set; }
    }
}
