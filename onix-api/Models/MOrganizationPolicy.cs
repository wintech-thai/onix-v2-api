using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using IndexAttribute = Microsoft.EntityFrameworkCore.IndexAttribute;
using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.Models
{
    [ExcludeFromCodeCoverage]
    [Table("OrganizationPolicies")]

    [Index(nameof(OrgId), IsUnique = true)]

    public class MOrganizationPolicy
    {
        [Key]
        [Column("org_policy_id")]
        public Guid? Id { get; set; }

        [Column("org_id")]
        public string? OrgId { get; set; } /* org id ของ merchant ไม่ใช่ global เพราะอนาคต merchant เองก็ set ค่าพวกนี้ได้เช่นกัน */

        [Column("web_whitelist_ips")]
        public string? WebWhitelistIps { get; set; } /* Comma separated string */

        [Column("api_whitelist_ips")]
        public string? ApiWhitelistIps { get; set; } /* Comma separated string */

        [Column("web_blacklist_ips")]
        public string? WebBlacklistIps { get; set; } /* Comma separated string */

        [Column("api_blacklist_ips")]
        public string? ApiBlacklistIps { get; set; } /* Comma separated string */

        [Column("created_date")]
        public DateTime? CreatedDate { get; set; }

        public MOrganizationPolicy()
        {
            Id = Guid.NewGuid();
            CreatedDate = DateTime.UtcNow;
        }
    }
}
