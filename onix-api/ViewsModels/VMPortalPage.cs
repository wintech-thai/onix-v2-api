using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.ViewsModels
{
    [ExcludeFromCodeCoverage]
    public class VMPortalPage : VMQueryBase
    {
        public string? FullTextSearch { get; set; } /* ค้นหาด้วย Code */
        public string? AuthMethod { get; set; }
        public string? Status { get; set; } /* Active, Disabled -> map ไปที่ IsEnabled */
    }
}
