using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.ViewsModels
{
    [ExcludeFromCodeCoverage]
    public class VMFileDocument : VMQueryBase
    {
        public string? FullTextSearch { get; set; }
        public string? DocumentType { get; set; }
    }
}
