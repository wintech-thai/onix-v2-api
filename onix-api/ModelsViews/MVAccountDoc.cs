using System.Diagnostics.CodeAnalysis;
using Its.Onix.Api.Models;

namespace Its.Onix.Api.ModelsViews
{
    [ExcludeFromCodeCoverage]
    public class MVAccountDoc
    {
        public string? Status { get; set; }
        public string? Description { get; set; }
        public MAccountDoc? AccountDoc { get; set; }
    }
}
