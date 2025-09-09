using System.Diagnostics.CodeAnalysis;
using Its.Onix.Api.Models;

namespace Its.Onix.Api.ModelsViews
{
    [ExcludeFromCodeCoverage]
    public class MVScanItemAction
    {
        public string? Status { get; set; }
        public string? Description { get; set; }
        public MScanItemAction? ScanItemAction { get; set; }

    }
}
