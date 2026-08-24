
namespace Its.Onix.Api.Models
{
    public class MCurrency
    {
        public string? CurrencyCoode { get; set; } //THB, USD, USDT, KAS, BTC
        public string? CurrencyName { get; set; }
        public string? Category { get; set; } //FIAT or CRYPTO

        // Currency properties
        public int Decimal { get; set; }
        public bool IsNative { get; set; }
        public string? Symbol { get; set; }

        public MCurrency()
        {
            Category = "CRYPTO";
        }
    }
}
