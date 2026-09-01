using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.ModelsViews
{
    [ExcludeFromCodeCoverage]
    public class MVIpPolicyCheck
    {
        public string? Status { get; set; }
        public string? Description { get; set; }

        public bool IsBlacklisted { get; set; }
        public string? ClientIp { get; set; }
        public string? WhitelistIps { get; set; }
        public string? BlacklistIps { get; set; }

        public MVIpPolicyCheck()
        {
        }
    }
}
