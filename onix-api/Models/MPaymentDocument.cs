using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using IndexAttribute = Microsoft.EntityFrameworkCore.IndexAttribute;
using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.Models
{
    [ExcludeFromCodeCoverage]
    [Table("PaymentDocuments")]

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
    [Index(nameof(RefId))]
    [Index(nameof(PaymentTransactionId))]

    public class MPaymentDocument : IOrgEntity
    {
        [Key]
        [Column("document_id")]
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

        [Column("file_document_id")]
        public string? FileDocumentId { get; set; } //Link ไปยัง File Document

        [Column("uploaded_file_path")]
        public string? UploadedFilePath { get; set; } //Path ของไฟล์ที่อัปโหลด

        [Column("ref_id")]
        public string? RefId { get; set; } //Reference ID สำหรับการอ้างอิงไฟล์

        [Column("payment_transaction_id")]
        public string? PaymentTransactionId { get; set; } //Reference ID สำหรับการอ้างอิงไฟล์


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
        [Column("payout_bank_account_id")]
        public string? PayOutBankAccountId { get; set; }

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


        [Column("reject_reason")]
        public string? RejectReason { get; set; }


        //System fields
        [Column("processing_messages")]
        public string? ProcessingMessages { get; set; } /* JSON string */


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
        public string? TxAmountStr { get; set; } //เอาไว้สำหรับ query

        [NotMapped]
        public string? MimeType { get; set; }

        [NotMapped]
        public string? DocumentType { get; set; }
        
        [NotMapped]
        public string? PreviewUrl { get; set; }

        [NotMapped]
        public string? PayInPromptPayId { get; set; } 

        [NotMapped]
        public string? PayInAccountType { get; set; }

        public MPaymentDocument()
        {
            Id = Guid.NewGuid();
            CreatedDate = DateTime.UtcNow;
            ProcessingSteps = [];
        }
    }
}
