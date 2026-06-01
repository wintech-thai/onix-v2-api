using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using IndexAttribute = Microsoft.EntityFrameworkCore.IndexAttribute;
using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.Models
{
    [ExcludeFromCodeCoverage]
    [Table("PaymentRequests")]

    [Index(nameof(OrgId))]
    [Index(nameof(RefId))]
    [Index(nameof(RefId1))]
    [Index(nameof(RefId2))]
    [Index(nameof(Direction))]
    [Index(nameof(Status))]
    [Index(nameof(BankAccountNo))]
    [Index(nameof(BankAccountName))]
    [Index(nameof(Direction))]
    [Index(nameof(CreatedDate))]
    [Index(nameof(PaymentTxId))]
    [Index(nameof(MerchantId))]
    [Index(nameof(GeneratedAmountStr))]

    public class MPaymentRequest
    {
        [Key]
        [Column("request_id")]
        public Guid? Id { get; set; }

        [Column("org_id")]
        public string? OrgId { get; set; }


        [Column("ref_id")]
        public string? RefId { get; set; } //รหัสอ้างอิงที่ลูกค้ากำหนดเอง, มีการ check unique ด้วย

        [Column("ref_id1")]
        public string? RefId1 { get; set; } //รหัสอ้างอิงที่ลูกค้ากำหนดเอง

        [Column("ref_id2")]
        public string? RefId2 { get; set; } //รหัสอ้างอิงที่ลูกค้ากำหนดเอง

        [Column("description")]
        public string? Description { get; set; } //คำอธิบายการชำระเงิน

        [Column("customer_email")]
        public string? CustomerEmail { get; set; }

        [Column("customer_phone")]
        public string? CustomerPhone { get; set; }

        [Column("currency")]
        public string? Currency { get; set; } //THB

        [Column("bank_code")]
        public string? BankCode { get; set; } //รหัสธนาคารของผู้โอน เช่น BAY, GSB, KTB, TTB 

        [Column("bank_account_no")]
        public string? BankAccountNo { get; set; } //หมายเลขบัญชีของผู้โอน 

        [Column("bank_account_name")]
        public string? BankAccountName { get; set; } //ชื่อบัญชีของผู้โอน 

        [Column("requested_amount")]
        public double? RequestedAmount { get; set; } //จำนวนเงิน > 0

        [Column("qr_provider")]
        public string? QrProvider { get; set; } //ธนาคารเจ้าของ QR code สำหรับให้ scan (PP = Promptpay)

        [Column("selected_payin_bank_account_id")]
        public string? SelectedPayInBankAccountId { get; set; } //Hidden field ใช้ระบุ PayIn bank account ID เข้ามาให้เอง


        [Column("payin_bank_id")]
        public string? PayinBankAccountId { get; set; } //Foreign key ไปยัง BankAccounts table

        [Column("payin_bank_code")]
        public string? PayinBankCode { get; set; }

        [Column("payin_bank_account_no")]
        public string? PayinBankAccountNo { get; set; }

        [Column("payin_bank_account_name")]
        public string? PayinBankAccountName { get; set; } 

        [Column("payin_promptpay_id")]
        public string? PayinPromptPayId { get; set; } 

        [Column("payin_account_type")]
        public string? PayinAccountType { get; set; }
         
        [Column("payin_account_level")]
        public string? PayinAccountLevel { get; set; }
        
        [Column("pay_in_fee_pct")]
        public double? PayInFeePct { get; set; } //เปอร์เซ็นค่าธรรมเนียมรับเข้า


        //PayOut fields - บัญชีที่เงินออกไปจ่ายให้ลูกค้า
        [Column("payout_bank_id")]
        public string? PayoutBankAccountId { get; set; } //Foreign key ไปยัง BankAccounts table

        [Column("payout_bank_code")]
        public string? PayoutBankCode { get; set; }

        [Column("payout_bank_account_no")]
        public string? PayoutBankAccountNo { get; set; }

        [Column("payout_bank_account_name")]
        public string? PayoutBankAccountName { get; set; } 

        [Column("payout_promptpay_id")]
        public string? PayoutPromptPayId { get; set; } 

        [Column("payout_account_type")]
        public string? PayoutAccountType { get; set; }
         
        [Column("payout_account_level")]
        public string? PayoutAccountLevel { get; set; }
        
        [Column("payout_fee_pct")]
        public double? PayoutFeePct { get; set; } //เปอร์เซ็นค่าธรรมเนียมจ่ายออก
        [Column("payout_fee_decimal")]

        public decimal? PayoutFeeDecimal { get; set; } //ค่าธรรมเนียมจ่ายออกเป็น decimal

        [Column("total_payout_amount_decimal")]
        public decimal? PayOutTotalAmountDecimal { get; set; } //ค่าธรรมเนียมจ่ายออกเป็น decimal


        //ด้านล่างเป็น field ที่ใช้กันภายใน
        [Column("tags")]
        public string? Tags { get; set; }

        [Column("status")]
        public string? Status { get; set; } //Pending - ยังไม่มีการจ่ายเงินเข้ามา, Paid - มีการจ่ายเงินเข้ามาแล้ว

        [Column("direction")]
        public string? Direction { get; set; } //PayIn, PayOut

        [Column("merchant_id")]
        public string? MerchantId { get; set; } //เอาไว้ join หาข้อมูล merchant

        [Column("merchant_id2")]
        public Guid MerchantId2 { get; set; } //เอาไว้ join หาข้อมูล merchant

        [Column("payment_tx_id")]
        public string? PaymentTxId { get; set; } //เอาไว้ link กับ payment transaction ที่เกิดขึ้นจริง


        [Column("generated_amount")]
        public double? GeneratedAmount { get; set; } //จำนวนเงินที่เอา RequestedAmount มาทำการ random เศษสตางค์

        [Column("generated_amount_str")]
        public string? GeneratedAmountStr { get; set; } //จำนวนเงินเป็น string เอาไว้ match กับ transaction


        [Column("response_data")]
        public string? ResponseData { get; set; } /* JSON string */

        [Column("processing_messages")]
        public string? ProcessingMessages { get; set; } /* JSON string */

        [Column("reject_reason")]
        public string? RejectReason { get; set; }

        [Column("qr_code")]
        public string? QrCode { get; set; }


        //System fields
        [Column("created_date")]
        public DateTime? CreatedDate { get; set; }

        [Column("expire_date")]
        public DateTime? ExpireDate { get; set; }




        [NotMapped]
        public string? MerchantName { get; set; }
        [NotMapped]
        public string? MerchantCode { get; set; }
        [NotMapped]
        public double? MerchantMinPayout { get; set; }
        [NotMapped]
        public double? MerchantMaxPayout { get; set; }


        [NotMapped]
        public MPaymentResponse? ResponseDataObj { get; set; }
        [NotMapped]
        public List<string>? ProcessingSteps { get; set; }

        public MPaymentRequest()
        {
            Id = Guid.NewGuid();
            CreatedDate = DateTime.UtcNow;
            ProcessingSteps = [];
        }
    }
}
