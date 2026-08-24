using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.ModelsViews
{
    [ExcludeFromCodeCoverage]
    public class MPayInSlipItem
    {
        public string? SlipId { get; set; }
        public string? ImageBase64 { get; set; }
        public DateTime? UploadedAt { get; set; }
        public string? First4 { get; set; }
        public string? Last4 { get; set; }
        public string? Note { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class MVPayInSlipUploads
    {
        public string? Status { get; set; }
        public string? Description { get; set; }
        public List<MPayInSlipItem>? Slips { get; set; }
        public string? SlipUploadUrl { get; set; }
    }
}
