using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.ViewsModels
{
    [ExcludeFromCodeCoverage]
    public class VMPaymentConfirmation
    {
        public string? TransactionId { get; set; }
        public decimal? Amount { get; set; }
        // Ref1/Ref2/Ref3 ต้องตรงกับที่ PointTopUpService.CreatePointTopUp ส่งไปตอนขอ QR (Ref1=accountDocId, Ref2="Point Purchase", Ref3=orgId)
        public string? Ref1 { get; set; }
        public string? Ref2 { get; set; }
        public string? Ref3 { get; set; }
    }
}
