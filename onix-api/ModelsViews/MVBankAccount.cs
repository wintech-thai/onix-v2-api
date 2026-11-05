using System.Diagnostics.CodeAnalysis;
using Its.Onix.Api.Models;

namespace Its.Onix.Api.ModelsViews
{
    [ExcludeFromCodeCoverage]
    public class MVBankAccount
    {
        public string? Status { get; set; }
        public string? Description { get; set; }
        public MBankAccount? BankAccount { get; set; }
        public string? BookBankImageBase64 { get; set; } // ใส่ค่าเฉพาะตอน GetBankAccountById ถ้ามีไฟล์แนบ
        public string? BookBankMimeType { get; set; }
    }
}
