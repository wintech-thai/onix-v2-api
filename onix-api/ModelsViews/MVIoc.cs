using System.Diagnostics.CodeAnalysis;
using Its.Onix.Api.Models;

namespace Its.Onix.Api.ModelsViews
{
    [ExcludeFromCodeCoverage]
    public class MVIoc
    {
        public string? Status { get; set; }
        public string? Description { get; set; }

        public MIoc? Ioc { get; set; }

        public MVIoc()
        {
        }
    }
}
