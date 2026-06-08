using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.ViewsModels
{
    [ExcludeFromCodeCoverage]
    public class VMNotiChannel : VMQueryBase
    {
        public string? FullTextSearch { get; set; }
        public string? Status { get; set; }
        public string? Type { get; set; }
    }
}
