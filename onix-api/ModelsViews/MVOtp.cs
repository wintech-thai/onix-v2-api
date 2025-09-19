using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.ModelsViews
{
    [ExcludeFromCodeCoverage]
    public class MVOtp
    {
        public string? Status { get; set; }
        public string? Description { get; set; }
        public string? OTP { get; set; }
    }
}
