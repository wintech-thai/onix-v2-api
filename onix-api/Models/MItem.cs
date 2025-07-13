using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using IndexAttribute = Microsoft.EntityFrameworkCore.IndexAttribute;
using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.Models
{
    [ExcludeFromCodeCoverage]
    [Table("Items")]

    [Index(nameof(OrgId))]
    public class MItem
    {
        [Key]
        [Column("item_id")]
        public Guid? Id { get; set; }

        [Column("org_id")]
        public string? OrgId { get; set; }

        [Column("code")]
        public string? Code { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        [Column("tags")]
        public string? Tags { get; set; }

        [Column("item_type")] /* 1=Lottery */
        public int? ItemType { get; set; }

        [Column("narrative")]
        public string? Narrative { get; set; }


        //System fields
        [Column("created_date")]
        public DateTime? CreatedDate { get; set; }

        [Column("updated_date")]
        public DateTime? UpdatedDate { get; set; }

        public MItem()
        {
            Id = Guid.NewGuid();
            CreatedDate = DateTime.UtcNow;
            UpdatedDate = DateTime.UtcNow;
        }
    }
}
