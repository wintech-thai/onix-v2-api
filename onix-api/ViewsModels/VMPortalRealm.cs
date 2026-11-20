using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.ViewsModels
{
    [ExcludeFromCodeCoverage]
    public class VMPortalRealm : VMQueryBase
    {
        public string? FullTextSearch { get; set; } /* ค้นหาด้วย RealmName */
        public string? AuthType { get; set; }
    }
}
