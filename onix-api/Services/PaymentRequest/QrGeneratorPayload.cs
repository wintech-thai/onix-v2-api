
namespace Its.Onix.Api.Services
{
    public class QrGeneratorPayload
    {
        public string TargetId { get; set; } = string.Empty; //PromptPay ID / Mobile / Tax ID / Wallet ID
        public double? Amount { get; set; }
        public string? Reference1 { get; set; }
        public string? Reference2 { get; set; }
        public string? AccountName { get; set; }
        public string CountryCode { get; set; } = "TH";
        public string CurrencyCode { get; set; } = "764"; // THB
        public object? ExtraData { get; set; }
        public string MerchantCategoryCode { get; set; } = "0000";
        public string TransactionId { get; set; } = Guid.NewGuid().ToString("N");
    }
}
