using System.Diagnostics.CodeAnalysis;
using Its.Onix.Api.Models;

namespace Its.Onix.Api.ModelsViews
{
    [ExcludeFromCodeCoverage]
    public class MVEntity
    {
        public string? Status { get; set; }
        public string? Description { get; set; }
        public MEntity? Entity { get; set; }
    }
}
