using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.ViewsModels
{
    [ExcludeFromCodeCoverage]
    public class VMPortalRealmUser : VMQueryBase
    {
        public string? FullTextSearch { get; set; } /* ค้นหาด้วย UserName หรือ Email */
    }
}
