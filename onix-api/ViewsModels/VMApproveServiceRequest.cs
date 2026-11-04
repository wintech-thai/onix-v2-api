using System.Diagnostics.CodeAnalysis;
using Its.Onix.Api.Models;

namespace Its.Onix.Api.ViewsModels
{
    [ExcludeFromCodeCoverage]
    public class VMApproveServiceRequest
    {
        public MOrganization? Organization { get; set; }
        public MBankAccount? BankAccount { get; set; }
    }
}
