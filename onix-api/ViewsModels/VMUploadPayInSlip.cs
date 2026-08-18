using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.ViewsModels
{
    [ExcludeFromCodeCoverage]
    public class VMUploadPayInSlip
    {
        public string? ImageBase64 { get; set; }
        public string? First4 { get; set; }
        public string? Last4 { get; set; }
        public string? Note { get; set; }
    }
}
