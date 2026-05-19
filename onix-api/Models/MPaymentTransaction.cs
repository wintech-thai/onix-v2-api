using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using IndexAttribute = Microsoft.EntityFrameworkCore.IndexAttribute;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Its.Onix.Api.Models
{
    [ExcludeFromCodeCoverage]
    [Table("PaymentTransactions")]

    [Index(nameof(OrgId))]
    [Index(nameof(PaymentRequestId))]
    [Index(nameof(Status))]
    [Index(nameof(Direction))]
    [Index(nameof(Direction))]
    [Index(nameof(Status))]
    [Index(nameof(PayInBankAccountId))]
    [Index(nameof(Direction))]
    [Index(nameof(CreatedDate))]
    [Index(nameof(MerchantId))]

    public class MPaymentTransaction
    {
        [Key]
        [Column("transaction_id")]
        public Guid? Id { get; set; }

        [Column("org_id")]
        public string? OrgId { get; set; }

        [Column("merchant_id")]
        public string? MerchantId { get; set; } //เอาไว้ join หาข้อมูล merchant

        [Column("payment_request_id")]
        public string? PaymentRequestId { get; set; } //Link ไปยัง Payment Request

        [Column("description")]
        public string? Description { get; set; } //คำอธิบายการชำระเงิน

        [Column("currency")]
        public string? Currency { get; set; } //THB

        [Column("tags")]
        public string? Tags { get; set; }

        [Column("status")]
        public string? Status { get; set; } //Identified, UnIdentified, Error

        [Column("direction")]
        public string? Direction { get; set; } //PayIn, PayOut

        //เกี่ยวกับ transaction ยอดเงิน
        [Column("tx_amount")]
        public double? TxAmount { get; set; } //จำนวนเงินเข้ามาจริง ๆ

        [Column("tx_amount_decimal")]
        public decimal? TxAmountDecimal { get; set; } //จำนวนเงินเข้ามาจริง ๆ

        [Column("pay_in_fee_pct")]
        public double? PayInFeePct { get; set; } //เปอร์เซ็นค่าธรรมเนียมรับเข้า

        [Column("pay_in_fee")]
        public double? PayInFee { get; set; } //ค่าธรรมเนียมรับเข้า

        [Column("pay_in_fee_decimal")]
        public decimal? PayInFeeDecimal { get; set; } //ค่าธรรมเนียมรับเข้า

        [Column("pay_out_fee_pct")]
        public double? PayOutFeePct { get; set; } //เปอร์เซ็นค่าธรรมเนียม withdraw

        [Column("pay_out_fee")]
        public double? PayOutFee { get; set; } //ค่าธรรมเนียม withdraw

        [Column("total_payin_amount")]
        public double? PayInTotalAmount { get; set; } //ยอดเงินที่เข้าไปที่ merchantจริง ๆ

        [Column("total_payin_amount_decimal")]
        public decimal? PayInTotalAmountDecimal { get; set; } //ยอดเงินที่เข้าไปที่ merchantจริง ๆ


        [Column("total_payout_amount")]
        public double? PayOutTotalAmount { get; set; } //ยอดเงินที่โอนออกไปให้ merchantจริง ๆ


        //ข้อมูลเกี่ยวกับ บัญชีที่รับเงินโอนเข้ามา
        [Column("payin_bank_account_id")]
        public string? PayInBankAccountId { get; set; }

        [Column("payin_bank_code")]
        public string? PayInBankCode { get; set; }

        [Column("payin_bank_account_no")]
        public string? PayInBankAccountNo { get; set; }

        [Column("payin_bank_account_name")]
        public string? PayInBankAccountName { get; set; }

        //ข้อมูลเกี่ยวกับ บัญชีที่โอนออกไปปลายทาง
        [Column("payout_bank_code")]
        public string? PayOutBankCode { get; set; }

        [Column("payout_bank_account_no")]
        public string? PayOutBankAccountNo { get; set; }

        [Column("payout_bank_account_name")]
        public string? PayOutBankAccountName { get; set; }


        //ข้อมูลบัญชีต้นทางของผู้โอน
        [Column("from_bank_code")]
        public string? FromBankCode { get; set; }

        [Column("from_bank_account_no")]
        public string? FromBankAccountNo { get; set; }

        [Column("from_bank_account_name")]
        public string? FromBankAccountName { get; set; }


        //System fields
        [Column("processing_messages")]
        public string? ProcessingMessages { get; set; } /* JSON string */

        [Column("raw_input")]
        public string? RawInput { get; set; } /* JSON string */


        [Column("created_date")]
        public DateTime? CreatedDate { get; set; }

        //#### Not map fields #########
        [NotMapped]
        public string? MerchantName { get; set; }

        [NotMapped]
        public string? MerchantCode { get; set; }

        [NotMapped]
        public List<string>? ProcessingSteps { get; set; }

        [NotMapped]
        public JsonElement? RawInputObj { get; set; }


        public MPaymentTransaction()
        {
            Id = Guid.NewGuid();
            CreatedDate = DateTime.UtcNow;
            ProcessingSteps = [];
        }
    }
}
