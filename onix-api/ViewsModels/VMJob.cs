using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.ViewsModels
{
    [ExcludeFromCodeCoverage]
    public class VMJob : VMQueryBase
    {
        public string? FullTextSearch { get; set; }
        public string? JobType { get; set; }
        public string? ScanItemTemplateId { get; set; }
    }
}
