
namespace Its.Onix.Api.Models
{
    public class MPartialPayout
    {
        public DateTime? TxDate { get; set; }
        public string? PayinRequestId { get; set; }
        public decimal? PartialAmount { get; set; }
        public string? Status { get; set; } /* Pending, Approved, Rejected */

        public MPartialPayout()
        {
            Status = "Pending";
            PartialAmount = 0;
        }
    }
}
