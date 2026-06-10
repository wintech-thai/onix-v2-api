using System.Diagnostics.CodeAnalysis;
using Its.Onix.Api.Models;

namespace Its.Onix.Api.ModelsViews
{
    [ExcludeFromCodeCoverage]
    public class MVEndPoint
    {
        public string? Status { get; set; }
        public string? Description { get; set; }
        public string? PaymentRequestUrl { get; set; }
        public string? PaymentTxNotiUrl { get; set; }
        public string? AgentHeartbeatUrl { get; set; }
        public string? AgentPaymentTxNotiUrl { get; set; }
    }
}
