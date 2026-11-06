namespace Its.Onix.Api.Services
{
    public class PaymentGatewayQrRequest
    {
        public string? Ref1 { get; set; }
        public string? Ref2 { get; set; }
        public string? Payer { get; set; } /* email ของผู้จ่าย */
        public decimal Amount { get; set; }
    }
}
