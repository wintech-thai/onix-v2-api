using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using IndexAttribute = Microsoft.EntityFrameworkCore.IndexAttribute;
using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.Models
{
    //เก็บเฉพาะ user ที่เป็น admin ใหญ่
    [ExcludeFromCodeCoverage]
    [Table("AdminUsers")]
    [Index(nameof(UserName), IsUnique = true)]

    public class MAdminUser
    {
        [Key]
        [Column("admin_user_id")]
        public Guid? AdminUserId { get; set; }

        [Column("user_id")]
        public string? UserId { get; set; } /* link ไปยัง Users อีกที */

        [Column("user_name")]
        public string? UserName { get; set; }

        [Column("created_date")]
        public DateTime? CreatedDate { get; set; }

        [Column("roles_list")]
        public string? RolesList { get; set; }

        [Column("user_status")]
        public string? UserStatus { get; set; } /* Pending, Active, Disabled */

        [Column("tmp_user_email")]
        public string? TmpUserEmail { get; set; } /* เก็บ email ชั่วคราวที่ได้ invite ไปหา user */

        [Column("previous_user_status")]
        public string? PreviousUserStatus { get; set; } /* เก็บ UserStatus ก่อนที่จะถูก Disabled ถ้า Enable ก็จะกลับมาใช้ PreviousUserStatus */

        [Column("invited_date")]
        public DateTime? InvitedDate { get; set; }

        [Column("invited_by")]
        public string? InvitedBy { get; set; }

        [Column("tags")]
        public string? Tags { get; set; }

        [NotMapped]
        public string? UserEmail { get; set; }
        [NotMapped]
        public List<string> Roles { get; set; }

        public MAdminUser()
        {
            AdminUserId = Guid.NewGuid();
            CreatedDate = DateTime.UtcNow;
            RolesList = "";
            Roles = [];
        }
    }
}
