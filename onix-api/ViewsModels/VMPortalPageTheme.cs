using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.ViewsModels
{
    [ExcludeFromCodeCoverage]
    public class VMPortalPageTheme : VMQueryBase
    {
        public string? FullTextSearch { get; set; } /* ค้นหาด้วย ThemeName */
        public string? AuthType { get; set; }
        public string? Language { get; set; }
    }
}
