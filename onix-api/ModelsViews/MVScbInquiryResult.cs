using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.ModelsViews
{
    [ExcludeFromCodeCoverage]
    public class MVScbInquiryResult
    {
        public string? Status { get; set; }
        public string? Description { get; set; }
        public string? RawResponse { get; set; }
    }
}
