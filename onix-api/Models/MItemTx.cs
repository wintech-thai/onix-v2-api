using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using IndexAttribute = Microsoft.EntityFrameworkCore.IndexAttribute;
using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.Models
{
    [ExcludeFromCodeCoverage]
    [Table("ItemTxs")]

    [Index(nameof(OrgId))]
    [Index(nameof(ItemId))]
    public class MItemTx
    {
        [Key]
        [Column("tx_id")]
        public Guid? Id { get; set; }

        [Column("org_id")]
        public string? OrgId { get; set; }

        [Column("item_id")]
        public string? ItemId { get; set; } /* Wallet ID (primary key of Items)*/

        [Column("tags")]
        public string? Tags { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        [Column("tx_amount")]
        public long? TxAmount { get; set; } /* Point tx amount */

        [Column("tx_type")]
        public int? TxType { get; set; } /* 1=IN, 2=OUT */


        [Column("current_balance")]
        public long? CurrentBalance { get; set; }

        [Column("previous_balance")]
        public long? PreviousBalance { get; set; }


        //System fields
        [Column("created_date")]
        public DateTime? CreatedDate { get; set; }

        [Column("updated_date")]
        public DateTime? UpdatedDate { get; set; }

        public MItemTx()
        {
            Id = Guid.NewGuid();
            CreatedDate = DateTime.UtcNow;
        }
    }
}
