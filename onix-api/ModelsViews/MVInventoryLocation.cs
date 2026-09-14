using System.Diagnostics.CodeAnalysis;
using Its.Onix.Api.Models;

namespace Its.Onix.Api.ModelsViews
{
    [ExcludeFromCodeCoverage]
    public class MVInventoryLocation
    {
        public string? Status { get; set; }
        public string? Description { get; set; }
        public MInventoryLocation? InventoryLocation { get; set; }
    }
}
