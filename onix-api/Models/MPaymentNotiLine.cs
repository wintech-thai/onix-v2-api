using System.Diagnostics.CodeAnalysis;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Models
{
    [ExcludeFromCodeCoverage]

    public class MPaymentNotiLine
    {
        public decimal? PaymentAmount { get; set; } //ยอดเงินเข้า
        public decimal? RemainAmount { get; set; } //เงินที่เหลือ - ใน Line จะแจ้งตรงนี้มาด้วย
        public string? TxType { get; set; } //PayIn, PayOut
        public string? SourceBankCode { get; set; } //ธนาคารต้นทาง
        public string? SourceBankAccountNo { get; set; } //XX3090 - จะ mask data แล้วแสดง 4 ตัวหลัง
        public string? DestinationBankCode { get; set; } //ธนาคารปลายทาง
        public string? DestinationAccountNo { get; set; } //XX9148 - จะ mask data แล้วแสดง 4 ตัวหลัง
        public string? MerchantId { get; set; }

        public string? RefId1 { get; set; }
        public Dictionary<string, object> OriginalData { get; set; }
        public VMPaymentRequest PaymentRequestQuery { get; set; }

        public DateTime? TxDate { get; set; }

        public MPaymentNotiLine()
        {
            TxDate = DateTime.UtcNow;
            OriginalData = [];
            PaymentRequestQuery = new();
        }
    }
}
