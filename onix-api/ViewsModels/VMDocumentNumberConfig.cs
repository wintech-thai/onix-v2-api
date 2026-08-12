using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.ViewsModels
{
    [ExcludeFromCodeCoverage]
    public class VMDocumentNumberConfig : VMQueryBase
    {
        public string? FullTextSearch { get; set; }
        public string? DocumentType { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class VMDocumentNumberSequence
    {
        public int CurrentSequenceNo { get; set; }
        public string? CurrentSequenceKey { get; set; }
    }
}
