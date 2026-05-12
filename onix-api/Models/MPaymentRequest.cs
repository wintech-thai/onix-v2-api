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
    [Index(nameof(Direction))]
    [Index(nameof(Status))]
    [Index(nameof(BankAccountNo))]
    [Index(nameof(BankAccountName))]
    [Index(nameof(Direction))]
    [Index(nameof(CreatedDate))]
    [Index(nameof(PaymentTxId))]
    [Index(nameof(MerchantId))]

    public class MPaymentRequest
    {
        [Key]
        [Column("request_id")]
        public Guid? Id { get; set; }

        [Column("org_id")]
        public string? OrgId { get; set; }

        [Column("ref_id")]
        public string? RefId { get; set; } //รหัสอ้างอิงที่ลูกค้ากำหนดเอง

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
        public string? QrProvider { get; set; } //ธนาคารเจ้าของ QR code สำหรับให้ scan


        //ด้านล่างเป็น field ที่ใช้กันภายใน
        [Column("tags")]
        public string? Tags { get; set; }

        [Column("status")]
        public string? Status { get; set; } //Pending - ยังไม่มีการจ่ายเงินเข้ามา, Paid - มีการจ่ายเงินเข้ามาแล้ว

        [Column("direction")]
        public string? Direction { get; set; } //PayIn, PayOut

        [Column("merchant_id")]
        public string? MerchantId { get; set; } //เอาไว้ join หาข้อมูล merchant

        [Column("payment_tx_id")]
        public string? PaymentTxId { get; set; } //เอาไว้ link กับ payment transaction ที่เกิดขึ้นจริง


        [Column("generated_amount")]
        public double? GeneratedAmount { get; set; } //จำนวนเงินที่เอา RequestedAmount มาทำการ random เศษสตางค์

        [Column("response_data")]
        public string? ResponseData { get; set; } /* JSON string */


        //System fields
        [Column("created_date")]
        public DateTime? CreatedDate { get; set; }

        [Column("expire_date")]
        public DateTime? ExpireDate { get; set; }

        [NotMapped]
        public string? MerchantName { get; set; }
        [NotMapped]
        public string? MerchantCode { get; set; }

        public MPaymentRequest()
        {
            Id = Guid.NewGuid();
            CreatedDate = DateTime.UtcNow;
        }
    }
}
