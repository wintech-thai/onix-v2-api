using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.ViewsModels
{
    [ExcludeFromCodeCoverage]
    public class VMAccountDoc : VMQueryBase
    {
        public string? FullTextSearch { get; set; }
        public string? DocumentType { get; set; }
        public string? DocumentStatus { get; set; }
        public string? EntityId { get; set; }
    }
}
