
namespace Its.Onix.Api.Models
{
    public class MBrandConfig
    {
        public string? BrandName { get; set; }
        public string? DocumentId { get; set; }
        public string? LogoPath { get; set; } // Legacy MinIO object path — kept for backward-compat migration only
        public string? LogoMimeType { get; set; }
        public string? LogoImageUrl { get; set; }
        public string? ThemeName { get; set; }

        // Transient: raw base64 image content sent by the client on upload.
        // Never persisted into ConfigValue — moved into a FileDocument's FileContent instead.
        public string? LogoBase64 { get; set; }

        public MBrandConfig()
        {
            BrandName = "PLEASE PAYMENT";
            LogoPath = "DEFAULT";
            LogoMimeType = "";
            ThemeName = "DEFAULT";
        }
    }
}
