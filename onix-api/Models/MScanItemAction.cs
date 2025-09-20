using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using IndexAttribute = Microsoft.EntityFrameworkCore.IndexAttribute;
using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.Models
{
    [ExcludeFromCodeCoverage]
    [Table("ScanItemActions")]

    [Index(nameof(OrgId))]
    public class MScanItemAction
    {
        [Key]
        [Column("action_id")]
        public Guid? Id { get; set; }

        [Column("org_id")]
        public string? OrgId { get; set; }

        [Column("redirect_url")]
        public string? RedirectUrl { get; set; }

        [Column("encryption_key")] 
        public string? EncryptionKey { get; set; }

        [Column("encryption_iv")] 
        public string? EncryptionIV { get; set; }

        [Column("theme_verify")] 
        public string? ThemeVerify { get; set; }

        //System fields
        [Column("created_date")]
        public DateTime? CreatedDate { get; set; }

        public MScanItemAction()
        {
            ThemeVerify = "default";
            Id = Guid.NewGuid();
            CreatedDate = DateTime.UtcNow;
        }
    }
}
