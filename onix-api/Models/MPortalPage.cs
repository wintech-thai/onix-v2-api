using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using IndexAttribute = Microsoft.EntityFrameworkCore.IndexAttribute;
using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.Models
{
    [ExcludeFromCodeCoverage]
    [Table("PortalPages")]
    [Index(nameof(OrgId))]
    [Index(nameof(OrgId), nameof(Code), IsUnique = true, Name = "PortalPage_Org_Code_Unique")]
    public class MPortalPage
    {
        [Key]
        [Column("portal_page_id")]
        public Guid? PortalPageId { get; set; }

        [Column("org_id")]
        public string? OrgId { get; set; }

        [Column("code")]
        public string? Code { get; set; }

        [Column("auth_method")]
        public string? AuthMethod { get; set; } /* Google, Native */

        [Column("theme")]
        public string? Theme { get; set; } /* Default */

        [Column("is_default")]
        public bool? IsDefault { get; set; }

        [Column("is_enabled")]
        public bool? IsEnabled { get; set; }

        [Column("api_key_id")]
        public Guid? ApiKeyId { get; set; } /* FK ไปยัง ApiKeys.key_id ไม่ได้เก็บค่า key จริงไว้ที่นี่ */

        [Column("api_code")]
        public string? ApiCode { get; set; } /* denormalized จาก ApiKeys.key_name เอาไว้แสดงเฉย ๆ */

        [Column("created_date")]
        public DateTime? CreatedDate { get; set; }

        public MPortalPage()
        {
            PortalPageId = Guid.NewGuid();
            CreatedDate = DateTime.UtcNow;
            IsEnabled = true;
            IsDefault = false;
        }
    }
}
