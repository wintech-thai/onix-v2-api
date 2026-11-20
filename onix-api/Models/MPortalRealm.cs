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

        [Column("created_date")]
        public DateTime? CreatedDate { get; set; }

        //นับจาก PortalPages ที่ผูก realm นี้อยู่ ไม่ persist ลง DB
        [NotMapped]
        public int? PortalPageCount { get; set; }

        public MPortalRealm()
        {
            RealmId = Guid.NewGuid();
            CreatedDate = DateTime.UtcNow;
        }
    }
}
