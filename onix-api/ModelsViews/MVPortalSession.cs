using System.Diagnostics.CodeAnalysis;
using Its.Onix.Api.Models;

namespace Its.Onix.Api.ModelsViews
{
    [ExcludeFromCodeCoverage]
    public class MVPortalSession
    {
        public string? Status { get; set; }
        public string? Description { get; set; }
        public MPortalSession? PortalSession { get; set; }
        public List<MPortalSession>? PortalSessions { get; set; } /* ใช้ตอน list */

        public MPortalPage? PortalPage { get; set; } /* แนบมาตอนสร้าง session จาก public endpoint ให้ frontend เรนเดอร์หน้า login */

        public string? RedirectUrl { get; set; } /* แนบมาตอน login สำเร็จ ให้ frontend เอาไป redirect กลับ Mikrotik */
    }
}
