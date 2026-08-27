using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.ViewsModels
{
    [ExcludeFromCodeCoverage]
    public class VMIoc : VMQueryBase
    {
        public string? FullTextSearch { get; set; }
        public string? IocType { get; set; }
        public string? Reputation { get; set;}
        public string? Status { get; set;}
    }
}
