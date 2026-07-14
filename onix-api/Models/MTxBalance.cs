
namespace Its.Onix.Api.Models
{
    public class MTxBalance
    {
        public int TxCount { get; set; }
        public decimal TxAmount { get; set; }

        public MTxBalance()
        {
            TxCount = 0;
            TxAmount = 0;
        }
    }
}
