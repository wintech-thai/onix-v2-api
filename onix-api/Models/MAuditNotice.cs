using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using IndexAttribute = Microsoft.EntityFrameworkCore.IndexAttribute;
using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.Models
{
    [ExcludeFromCodeCoverage]
    [Table("AuditNotices")]

    [Index(nameof(OrgId))]
    [Index(nameof(RowId))]
    [Index(nameof(TrackModel))]

    public class MAuditNotice
    {
        [Key]
        [Column("notice_id")]
        public Guid? Id { get; set; }

        [Column("org_id")]
        public string? OrgId { get; set; }

        [Column("track_model")]
        public string? TrackModel { get; set; } /* Model หรือ ชื่อ table นั่นแหละ */

        [Column("row_id")]
        public string? RowId { get; set; } /* ID ของ row นั้น ๆ ที่เราจะ track */

        [Column("message")]
        public string? Message { get; set; } //ข้อความ warning

        //System fields
        [Column("created_date")]
        public DateTime? CreatedDate { get; set; }


        public MAuditNotice()
        {
            Id = Guid.NewGuid();
            CreatedDate = DateTime.UtcNow;
        }
    }
}
