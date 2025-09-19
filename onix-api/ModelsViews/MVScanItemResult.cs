using System.Diagnostics.CodeAnalysis;
using Its.Onix.Api.Models;

namespace Its.Onix.Api.ModelsViews
{
    [ExcludeFromCodeCoverage]
    public class MVScanItemResult
    {
        public string? Status { get; set; }
        public string? DescriptionThai { get; set; }
        public string? DescriptionEng { get; set; }

        public MScanItem? ScanItem { get; set; }
        public string? RedirectUrl { get; set; }
        public string? GetProductUrl { get; set; }
        public string? GetCustomerUrl { get; set; }
        public string? RegisterCustomerUrl { get; set; }
        public string? RequestOtpViaEmailUrl { get; set; }

        /* Object generated date, to determine if this data is too old */
        public DateTime? DataGeneratedDate { get; set; }
        public int? TtlMinute { get; set; }

        public MVScanItemResult()
        {
            DataGeneratedDate = DateTime.UtcNow;
            TtlMinute = 5; //5 minutes TTL since DataGeneratedDate
        }
    }
}
