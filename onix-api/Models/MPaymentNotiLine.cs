using System.Diagnostics.CodeAnalysis;

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

        public DateTime? TxDate { get; set; }

        public MPaymentNotiLine()
        {
            TxDate = DateTime.UtcNow;
        }
    }
}
