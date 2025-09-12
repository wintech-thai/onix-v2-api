using System.Diagnostics.CodeAnalysis;
using Its.Onix.Api.Models;

namespace Its.Onix.Api.ModelsViews
{
    [ExcludeFromCodeCoverage]
    public class MVScanItem
    {
        public string? Status { get; set; }
        public string? Description { get; set; }

        public MScanItem? ScanItem { get; set; }

        public MVScanItem()
        {
        }
    }
}
