
namespace Its.Onix.Api.Models
{
    public class MCurrency
    {
        public string? CurrencyCoode { get; set; } //THB, USD, USDT, KAS, BTC
        public string? CurrencyName { get; set; }
        public string? Category { get; set; } //FIAT or CRYPTO

        public MCurrency()
        {
            Category = "CRYPTO";
        }
    }
}
