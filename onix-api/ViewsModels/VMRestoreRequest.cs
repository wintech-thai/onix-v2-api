using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.ViewsModels
{
    [ExcludeFromCodeCoverage]
    public class VMRestoreRequest
    {
        public string? Filename { get; set; }
        public string? Bucket   { get; set; }
        public string? Folder   { get; set; }
    }
}
