using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.ViewsModels
{
    [ExcludeFromCodeCoverage]
    public class VMPaymentTransaction : VMQueryBase
    {
        public string? FullTextSearch { get; set; }
        public string? Direction { get; set; }
        public string? Status { get; set; }
        public bool? IsPeerToPeer { get; set; }

        //x088 : merchant แบบ MerchantOwn ให้เห็น Unidentified tx (OrgId ยังเป็น global) ของ bank account ตัวเองได้ด้วย แม้ยังไม่ approve
        public List<string>? IncludeUnidentifiedForBankAccountIds { get; set; }
    }
}
