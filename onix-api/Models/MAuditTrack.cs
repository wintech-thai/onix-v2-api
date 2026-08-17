using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using IndexAttribute = Microsoft.EntityFrameworkCore.IndexAttribute;
using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.Models
{
    [ExcludeFromCodeCoverage]
    [Table("AuditTracks")]

    [Index(nameof(OrgId))]
    [Index(nameof(RowId))]
    [Index(nameof(TrackModel))]

    public class MAuditTrack
    {
        [Key]
        [Column("track_id")]
        public Guid? Id { get; set; }

        [Column("org_id")]
        public string? OrgId { get; set; }

        [Column("track_model")]
        public string? TrackModel { get; set; } /* Model หรือ ชื่อ table นั่นแหละ */

        [Column("row_id")]
        public string? RowId { get; set; } /* ID ของ row นั้น ๆ ที่เราจะ track */

        [Column("action_name")]
        public string? ActionName { get; set; } /* Added, Modified, Delete */

        [Column("action_requested")]
        public string? ActionRequested { get; set; } /* อาจจะเป็นชื่อ path API ก็ได้ */

        [Column("action_by")]
        public string? ActionBy { get; set; } /* ชื่อ user ที่ทำ action นั้น */

        [Column("ip_address")]
        public string? IpAddress { get; set; } /* เพื่อตอบคำถาม where */

        [Column("ip_address2")]
        public string? IpAddress2 { get; set; } /* เพื่อตอบคำถาม where */

        [Column("old_value")]
        public string? OldValue { get; set; } /* JSON string ที่เก็บ state เดิมก่อน change */

        [Column("new_value")]
        public string? NewValue { get; set; } /* JSON string ที่เก็บ state เดิมหลัง change */

        [Column("api_name")]
        public string? ApiName { get; set; } /* ชื่อ API */


        //System fields
        [Column("created_date")]
        public DateTime? CreatedDate { get; set; }


        public MAuditTrack()
        {
            Id = Guid.NewGuid();
            CreatedDate = DateTime.UtcNow;
        }
    }
}
