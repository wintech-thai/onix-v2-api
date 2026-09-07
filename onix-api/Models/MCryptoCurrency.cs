namespace Its.Onix.Api.Models
{
    // Reference catalog of crypto currencies available to pick from when adding a
    // new Crypto Currency Account (see AdminCurrencyAccountController.GetAvailableCryptoCurrencies).
    // Mirrors the MBank catalog pattern — a hardcoded in-memory list, not a DB table.
    public class MCryptoCurrency
    {
        public string? Code { get; set; }            // e.g. "BTC", "KAS", "USDT"
        public string? Name { get; set; }             // e.g. "Bitcoin", "Kaspa", "Tether"
        public string DefaultNetwork { get; set; }     // Suggested CryptoWalletNetwork default, e.g. "BITCOIN", "TRON"
        public int DefaultDecimal { get; set; }        // Suggested CryptoDecimal default, e.g. 8 for BTC, 6 for USDT
        public bool IsToken { get; set; }              // true for token currencies riding on another chain (needs CryptoTokenContract), e.g. USDT/USDC

        public MCryptoCurrency()
        {
            DefaultNetwork = "";
            DefaultDecimal = 6;
            IsToken = false;
        }
    }
}
