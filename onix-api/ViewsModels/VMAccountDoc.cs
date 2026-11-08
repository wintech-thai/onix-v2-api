using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.ViewsModels
{
    [ExcludeFromCodeCoverage]
    public class VMAccountDoc : VMQueryBase
    {
        public string? FullTextSearch { get; set; }
        public string? DocumentType { get; set; }
        public string? DocumentSubType { get; set; }
        public string? DocumentStatus { get; set; }
        public string? EntityId { get; set; }

        // Admin cross-org search only (ดูทุก merchant พร้อมกัน) — ว่างไว้ถ้าอยากดูทุก org, ใส่ค่าถ้าอยากกรองดู org เดียว
        public string? OrgIdFilter { get; set; }
    }
}
