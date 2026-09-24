using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.ViewsModels
{
    [ExcludeFromCodeCoverage]
    public class VMInventoryDoc : VMQueryBase
    {
        public string? DocumentType { get; set; } /* StockIn, StockOut, StockTransfer */
        public string? FullTextSearch { get; set; }
        public string? DocumentStatus { get; set; } /* Pending, Approved, Cancelled */
    }
}
