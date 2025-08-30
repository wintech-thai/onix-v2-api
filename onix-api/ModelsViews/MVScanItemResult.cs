using System.Diagnostics.CodeAnalysis;
using Its.Onix.Api.Models;

namespace Its.Onix.Api.ModelsViews
{
    [ExcludeFromCodeCoverage]
    public class MVScanItemResult
    {
        public string? Status { get; set; }
        public string? DescriptionThai { get; set; }
        public string? DescriptionEng { get; set; }
        public MScanItem? ScanItem { get; set; }
    }
}
