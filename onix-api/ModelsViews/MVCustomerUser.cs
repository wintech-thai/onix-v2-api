using System.Diagnostics.CodeAnalysis;
using Its.Onix.Api.Models;

namespace Its.Onix.Api.ModelsViews
{
    [ExcludeFromCodeCoverage]
    public class MVCustomerUser
    {
        public string? Status { get; set; }
        public string? Description { get; set; }
        public MEntity? CustomerUser { get; set; }
        public MUser? User { get; set; }
    }
}
