using System.Diagnostics.CodeAnalysis;
using Its.Onix.Api.Models;

namespace Its.Onix.Api.ModelsViews
{
    [ExcludeFromCodeCoverage]
    public class MVPricingPlanItem
    {
        public string? Status { get; set; }
        public string? Description { get; set; }
        public List<MPricingPlanItem>? PricingPlanItems { get; set; }
        public MPricingPlanItem? PricingPlanItem { get; set; }
    }
}
