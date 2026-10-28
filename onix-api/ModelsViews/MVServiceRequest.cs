using System.Diagnostics.CodeAnalysis;
using Its.Onix.Api.Models;

namespace Its.Onix.Api.ModelsViews
{
    [ExcludeFromCodeCoverage]
    public class MVServiceRequest
    {
        public string? Status { get; set; }
        public string? Description { get; set; }
        public MServiceRequest? ServiceRequest { get; set; }
        public string? RegistrationLink { get; set; } // ใส่ค่าเฉพาะตอน Create*RegistrationLink เท่านั้น
        public string? BookBankImageBase64 { get; set; } // ใส่ค่าเฉพาะตอน GetServiceRequestById (admin) ถ้ามีไฟล์แนบ
        public string? BookBankMimeType { get; set; }
    }
}
