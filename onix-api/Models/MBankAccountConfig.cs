
namespace Its.Onix.Api.Models
{
    public class MBankAccountConfig
    {
        public bool IsSandbox { get; set; }
        public string? BillerId { get; set; }
        public string? ApiKey { get; set; }
        public string? ApiSecret { get; set; }
        public string? Ref3Prefix { get; set; }

        public MBankAccountConfig()
        {
        }
    }
}
