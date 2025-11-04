using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using IndexAttribute = Microsoft.EntityFrameworkCore.IndexAttribute;
using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.Models
{
    [ExcludeFromCodeCoverage]
    [Table("Wallets")]

    [Index(nameof(OrgId))]
    [Index(nameof(CustomerId))]
    public class MWallet
    {
        [Key]
        [Column("wallet_id")]
        public Guid? Id { get; set; }

        [Column("org_id")]
        public string? OrgId { get; set; }

        [Column("name")]
        public string? Name { get; set; }

        [Column("tags")]
        public string? Tags { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        [Column("customer_id")]
        public string? CustomerId { get; set; }

        [Column("point_balance")]
        public long? PointBalance { get; set; }

        //System fields
        [Column("created_date")]
        public DateTime? CreatedDate { get; set; }

        [Column("updated_date")]
        public DateTime? UpdatedDate { get; set; }

        public MWallet()
        {
            Id = Guid.NewGuid();
            CreatedDate = DateTime.UtcNow;
        }
    }
}
