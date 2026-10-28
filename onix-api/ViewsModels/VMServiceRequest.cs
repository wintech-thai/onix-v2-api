using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.ViewsModels
{
    [ExcludeFromCodeCoverage]
    public class VMServiceRequest : VMQueryBase
    {
        public string? FullTextSearch { get; set; }
        public string? ServiceType { get; set; }
        public string? Status { get; set; }
    }
}
