using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using IndexAttribute = Microsoft.EntityFrameworkCore.IndexAttribute;
using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.Models
{
    [ExcludeFromCodeCoverage]
    [Table("EntityAddresses")]

    [Index(nameof(OrgId))]
    [Index(nameof(EntityId))]
    public class MEntityAddress
    {
        [Key]
        [Column("entity_address_id")]
        public Guid? Id { get; set; }

        [Column("org_id")]
        public string? OrgId { get; set; }

        [Column("entity_id")]
        public string? EntityId { get; set; } /* UUID ไปยัง Entity */

        [Column("address_type")]
        public string? AddressType { get; set; } /* เช่น ที่อยู่จดทะเบียน, ที่อยู่ออกใบกำกับภาษี, ที่อยู่จัดส่งสินค้า */

        [Column("address")]
        public string? Address { get; set; }

        //System fields
        [Column("created_date")]
        public DateTime? CreatedDate { get; set; }

        [Column("updated_date")]
        public DateTime? UpdatedDate { get; set; }

        public MEntityAddress()
        {
            Id = Guid.NewGuid();
            CreatedDate = DateTime.UtcNow;
            UpdatedDate = DateTime.UtcNow;
        }
    }
}
