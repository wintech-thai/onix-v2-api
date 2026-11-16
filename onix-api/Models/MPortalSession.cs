using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using IndexAttribute = Microsoft.EntityFrameworkCore.IndexAttribute;
using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.Models
{
    [ExcludeFromCodeCoverage]
    [Table("PortalSessions")]
    [Index(nameof(OrgId))]
    public class MPortalSession
    {
        [Key]
        [Column("portal_session_id")]
        public Guid? PortalSessionId { get; set; }

        [Column("org_id")]
        public string? OrgId { get; set; }

        [Column("portal_page_code")]
        public string? PortalPageCode { get; set; } /* Code ของ Portal Page ที่ session นี้เกิดขึ้น */

        [Column("parameters_json", TypeName = "text")]
        public string? ParametersJson { get; set; } /* เก็บ parameter ทั้งหมดที่ส่งเข้ามาทางหน้า portal แบบ dynamic (form/query) */

        [Column("status")]
        public string? Status { get; set; } /* Pending, Failed, Authorized */

        [Column("status_desc")]
        public string? StatusDesc { get; set; } /* เหตุผล เช่น ตอน Failed เพราะ api key ไม่ถูก */

        [Column("created_date")]
        public DateTime? CreatedDate { get; set; }

        [Column("login_date")]
        public DateTime? LoginDate { get; set; } /* stamp ตอน status เปลี่ยนเป็น Authorized */

        public MPortalSession()
        {
            PortalSessionId = Guid.NewGuid();
            CreatedDate = DateTime.UtcNow;
            Status = "Pending";
        }
    }
}
