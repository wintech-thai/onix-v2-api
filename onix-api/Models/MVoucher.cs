using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using IndexAttribute = Microsoft.EntityFrameworkCore.IndexAttribute;
using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.Models
{
    [ExcludeFromCodeCoverage]
    [Table("Vouchers")]

    [Index(nameof(OrgId))]
    [Index(nameof(VoucherNo))]
    [Index(nameof(Pin))]
    [Index(nameof(Barcode))]

    public class MVoucher
    {
        [Key]
        [Column("voucher_id")]
        public Guid? Id { get; set; }

        [Column("org_id")]
        public string? OrgId { get; set; }

        [Column("voucher_no")]
        public string? VoucherNo { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        [Column("entity_id")]
        public string? CustomerId { get; set; }

        [Column("wallet_id")]
        public string? WalletId { get; set; }

        [Column("privilege_id")]
        public string? PrivilegeId { get; set; } /* อ้างอิงไปยัง MItem */

        [Column("tags")]
        public string? Tags { get; set; }

        [Column("start_date")]
        public DateTime? StartDate { get; set; }

        [Column("end_date")]
        public DateTime? EndDate { get; set; }

        [Column("redeem_price")]
        public double? RedeemPrice { get; set; } 

        [Column("status")] /* Disable, Active */
        public string? Status { get; set; }

        [Column("is_used")]
        public string? IsUsed { get; set; } /* ถูกใช้งานไปแล้วหรือยัง --> YES or NO */

        [Column("voucher_params")]
        public string? VoucherParams { get; set; } /* เป็น JSON string บอกว่า price calculate อย่างไร */

        [Column("piin")]
        public string? Pin { get; set; } /* password 6 หลัก */

        [Column("barcode")]
        public string? Barcode { get; set; } /* Code128 */
        
        [Column("used_date")]
        public DateTime? UsedDate { get; set; }

        //System fields
        [Column("created_date")]
        public DateTime? CreatedDate { get; set; }

        [Column("updated_date")]
        public DateTime? UpdatedDate { get; set; }

        [NotMapped]
        public string? CustomerCode { get; set; }
        [NotMapped]
        public string? CustomerName { get; set; }
        [NotMapped]
        public string? CustomerEmail { get; set; }
        [NotMapped]
        public string? PrivilegeCode { get; set; }
        [NotMapped]
        public string? PrivilegeName { get; set; }
        [NotMapped]
        public string? GetVoucherVerifyUrl { get; set; }

        public MVoucher()
        {
            Id = Guid.NewGuid();
            CreatedDate = DateTime.UtcNow;
            UpdatedDate = DateTime.UtcNow;
        }
    }
}
