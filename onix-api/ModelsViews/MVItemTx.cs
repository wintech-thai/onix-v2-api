using System.Diagnostics.CodeAnalysis;
using Its.Onix.Api.Models;

namespace Its.Onix.Api.ModelsViews
{
    [ExcludeFromCodeCoverage]
    public class MVItemTx
    {
        public string? Status { get; set; }
        public string? Description { get; set; }
        public MItemTx? ItemTx { get; set; }
    }
}
