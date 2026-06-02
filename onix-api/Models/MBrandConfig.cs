
namespace Its.Onix.Api.Models
{
    public class MBrandConfig
    {
        public string? BrandName { get; set; }
        public string? LogoPath { get; set; }
        public string? LogoImageUrl { get; set; }
        public string? ThemeName { get; set; }

        public MBrandConfig()
        {
            BrandName = "PLEASE PAYMENT";
            LogoPath = "DEFAULT";
            ThemeName = "DEFAULT";
        }
    }
}
