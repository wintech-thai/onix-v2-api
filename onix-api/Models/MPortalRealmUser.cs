using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using IndexAttribute = Microsoft.EntityFrameworkCore.IndexAttribute;
using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.Models
{
    [ExcludeFromCodeCoverage]
    [Table("PortalRealmUsers")]
    [Index(nameof(OrgId))]
    [Index(nameof(RealmId))]
    [Index(nameof(RealmId), nameof(UserName), IsUnique = true, Name = "PortalRealmUser_Realm_UserName_Unique")]
    public class MPortalRealmUser
    {
        [Key]
        [Column("realm_user_id")]
        public Guid? RealmUserId { get; set; }

        [Column("org_id")]
        public string? OrgId { get; set; }

        [Column("realm_id")]
        public Guid? RealmId { get; set; }

        [Column("user_name")]
        public string? UserName { get; set; }

        [Column("first_name")]
        public string? FirstName { get; set; }

        [Column("last_name")]
        public string? LastName { get; set; }

        [Column("email")]
        public string? Email { get; set; }

        [Column("encrypted_password", TypeName = "text")]
        public string? EncryptedPassword { get; set; }

        [Column("is_enabled")]
        public bool? IsEnabled { get; set; }

        [Column("created_date")]
        public DateTime? CreatedDate { get; set; }

        [Column("last_seen_date")]
        public DateTime? LastSeenDate { get; set; }

        //รหัสผ่านแบบ plaintext รับเข้ามาตอน add/reset เท่านั้น ไม่ persist ลง DB (hash ไปเก็บที่ EncryptedPassword แทน)
        [NotMapped]
        public string? Password { get; set; }

        public MPortalRealmUser()
        {
            RealmUserId = Guid.NewGuid();
            CreatedDate = DateTime.UtcNow;
            IsEnabled = true;
        }
    }
}
