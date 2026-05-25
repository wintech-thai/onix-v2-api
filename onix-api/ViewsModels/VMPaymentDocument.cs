using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.ViewsModels
{
    [ExcludeFromCodeCoverage]
    public class VMPaymentDocument : VMQueryBase
    {
        public string? FullTextSearch { get; set; }
        public string? Direction { get; set; }
        public string? Status { get; set; }
    }
}
