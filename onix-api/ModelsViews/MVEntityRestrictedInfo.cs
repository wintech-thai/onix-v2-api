using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.ModelsViews
{
    [ExcludeFromCodeCoverage]
    public class MVEntityRestrictedInfo
    {
        public string? Status { get; set; }
        public string? Description { get; set; }
        public string? MaskingEmail { get; set; }
    }
}
