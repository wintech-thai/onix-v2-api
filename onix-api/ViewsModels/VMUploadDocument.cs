using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.ViewsModels
{
    [ExcludeFromCodeCoverage]
    public class VMUploadDocument : VMQueryBase
    {
        public string? MimeType { get; set; }
    }
}
