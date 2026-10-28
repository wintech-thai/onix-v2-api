using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using IndexAttribute = Microsoft.EntityFrameworkCore.IndexAttribute;
using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.Models
{
    [ExcludeFromCodeCoverage]
    [Table("ServiceRequests")]

    [Index(nameof(OrgId))]
    [Index(nameof(ServiceType))]
    [Index(nameof(Status))]
    public class MServiceRequest
    {
        [Key]
        [Column("service_request_id")]
        public Guid? Id { get; set; }

        [Column("org_id")]
        public string? OrgId { get; set; } /* "global" เสมอตอนนี้ — ยังไม่ผูกกับ org ใด ๆ จนกว่าจะ approve */

        [Column("service_type")]
        public string? ServiceType { get; set; } /* CustomerRegistration, (future) NewBankAccount */

        [Column("status")]
        public string? Status { get; set; } /* Pending, Confirmed, Approved, Rejected */

        [Column("otp")]
        public string? Otp { get; set; }

        [Column("expired_date")]
        public DateTime? ExpiredDate { get; set; }

        [Column("lang")]
        public string? Lang { get; set; } /* th, en — ภาษาสำหรับอีเมล, default en ถ้าไม่ระบุ */

        [Column("admin_domain")]
        public string? AdminDomain { get; set; } /* origin ของ Admin ที่ผู้เรียก API ส่งมาตอนสร้าง link */

        // ข้อมูลเบื้องต้นตอนสร้าง link
        [Column("email")]
        public string? Email { get; set; }

        [Column("first_name")]
        public string? FirstName { get; set; }

        [Column("last_name")]
        public string? LastName { get; set; }

        [Column("phone")]
        public string? Phone { get; set; }

        // ข้อมูลที่ลูกค้ากรอกเพิ่มตอน confirm (CustomerRegistration)
        [Column("bank_code")]
        public string? BankCode { get; set; }

        [Column("bank_account_number")]
        public string? BankAccountNumber { get; set; }

        [Column("bank_account_name")]
        public string? BankAccountName { get; set; }

        [Column("prompt_pay_id")]
        public string? PromptPayId { get; set; }

        [Column("book_bank_file_document_id")]
        public string? BookBankFileDocumentId { get; set; } /* Ref ไปยัง MFileDocument.Id */

        [Column("shop_code")]
        public string? ShopCode { get; set; } /* ชื่อ/รหัสร้านค้า — กลายเป็นชื่อ Organization ตอน approve ด้วย */

        [Column("username")]
        public string? Username { get; set; } /* ลูกค้ากำหนดเอง */

        //System fields
        [Column("created_date")]
        public DateTime? CreatedDate { get; set; }

        [Column("updated_date")]
        public DateTime? UpdatedDate { get; set; }

        [Column("approved_date")]
        public DateTime? ApprovedDate { get; set; }

        public MServiceRequest()
        {
            Id = Guid.NewGuid();
            CreatedDate = DateTime.UtcNow;
            Status = "Pending";
        }
    }
}
