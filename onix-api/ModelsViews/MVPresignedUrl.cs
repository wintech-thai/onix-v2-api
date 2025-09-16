using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.ModelsViews
{
    [ExcludeFromCodeCoverage]
    public class MVPresignedUrl
    {
        public string? Status { get; set; }
        public string? Description { get; set; }
        public string? PresignedUrl { get; set; }
        public string? ObjectName { get; set; }
    }
}
