using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using IndexAttribute = Microsoft.EntityFrameworkCore.IndexAttribute;
using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.Models
{
    [ExcludeFromCodeCoverage]
    [Table("ItemImages")]

    [Index(nameof(OrgId))]
    public class MItemImage
    {
        [Key]
        [Column("item_image_id")]
        public Guid? Id { get; set; }

        [Column("org_id")]
        public string? OrgId { get; set; }

        [Column("item_id")]
        [ForeignKey("Item")]
        public Guid? ItemId { get; set; }
        public MItem? Item { get; set; }  // <== Navigation property

        [Column("path")]
        public string? ImagePath { get; set; }

        [Column("url")]
        public string? ImageUrl { get; set; }

        [Column("narative")]
        public string? Narative { get; set; }

        [Column("tags")]
        public string? Tags { get; set; }

        [Column("category")] 
        public int? Category { get; set; }

        [Column("sorting_order")]
        public int? SortingOrder { get; set; }

        //System fields
        [Column("created_date")]
        public DateTime? CreatedDate { get; set; }

        [Column("updated_date")]
        public DateTime? UpdatedDate { get; set; }

        public MItemImage()
        {
            Id = Guid.NewGuid();
            CreatedDate = DateTime.UtcNow;
            UpdatedDate = DateTime.UtcNow;
        }
    }
}
