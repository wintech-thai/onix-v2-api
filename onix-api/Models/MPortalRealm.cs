using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using IndexAttribute = Microsoft.EntityFrameworkCore.IndexAttribute;
using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.Models
{
    [ExcludeFromCodeCoverage]
    [Table("PortalRealms")]
    [Index(nameof(OrgId))]
    [Index(nameof(OrgId), nameof(RealmName), IsUnique = true, Name = "PortalRealm_Org_Name_Unique")]
    public class MPortalRealm
    {
        [Key]
        [Column("realm_id")]
        public Guid? RealmId { get; set; }

        [Column("org_id")]
        public string? OrgId { get; set; }

        [Column("realm_name")]
        public string? RealmName { get; set; }

        [Column("auth_type")]
        public string? AuthType { get; set; } /* Native, Google */

        [Column("description")]
        public string? Description { get; set; }

        [Column("google_client_id")]
        public string? GoogleClientId { get; set; } /* เฉพาะ AuthType = Google */

        [Column("google_client_secret")]
        public string? GoogleClientSecret { get; set; } /* เฉพาะ AuthType = Google */

        [Column("created_date")]
        public DateTime? CreatedDate { get; set; }

        //นับจาก PortalPages ที่ผูก realm นี้อยู่ ไม่ persist ลง DB
        [NotMapped]
        public int? PortalPageCount { get; set; }

        //นับจาก RealmUsers ที่ผูก realm นี้อยู่ ไม่ persist ลง DB (มีความหมายเฉพาะ realm ที่เป็น Native)
        [NotMapped]
        public int? RealmUserCount { get; set; }

        public MPortalRealm()
        {
            RealmId = Guid.NewGuid();
            CreatedDate = DateTime.UtcNow;
        }
    }
}
