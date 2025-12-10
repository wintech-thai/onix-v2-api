using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using IndexAttribute = Microsoft.EntityFrameworkCore.IndexAttribute;
using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.Models
{
    [ExcludeFromCodeCoverage]
    [Table("ScanItemFolders")]

    [Index(nameof(OrgId))]
    [Index(nameof(FolderName))]
    
    public class MScanItemFolder
    {
        [Key]
        [Column("folder_id")]
        public Guid? Id { get; set; }

        [Column("org_id")]
        public string? OrgId { get; set; }

        [Column("folder_name")]
        public string? FolderName { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        [Column("scanitem_action_id")]
        public string? ScanItemActionId { get; set; }

        [Column("tags")]
        public string? Tags { get; set; }

        [Column("scan_item_count")] 
        public int ScanItemCount { get; set; } /* นับว่ามี scan item อยู่กี่อันใน folder นี้ */

        //System fields
        [Column("created_date")]
        public DateTime? CreatedDate { get; set; }

        [NotMapped]
        public string? ScanItemActionName { get; set; }

        public MScanItemFolder()
        {
            Id = Guid.NewGuid();
            CreatedDate = DateTime.UtcNow;
        }
    }
}
