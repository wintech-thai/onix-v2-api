using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.ViewsModels
{
    [ExcludeFromCodeCoverage]
    public class VMRole : VMQueryBase
    {
        public string? FullTextSearch { get; set; }
    }
}
