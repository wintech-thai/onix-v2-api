using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.ViewsModels
{
    [ExcludeFromCodeCoverage]
    public class VMPricingPlanItem : VMQueryBase
    {
        public string? FullTextSearch { get; set; }
        public string? PricingPlanId { get; set; }
    }
}
