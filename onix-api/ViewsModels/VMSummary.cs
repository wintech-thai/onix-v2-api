using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.ViewsModels
{
    [ExcludeFromCodeCoverage]
    public class VMSummary : VMQueryBase
    {
        public bool NeedMerchantSummary { get; set; }

        public VMSummary()
        {
            NeedMerchantSummary = false;
        }
    }
}
