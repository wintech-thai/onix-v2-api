using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using IndexAttribute = Microsoft.EntityFrameworkCore.IndexAttribute;
using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.Models
{
    [ExcludeFromCodeCoverage]
    public class MRegistrationParam
    {
        public string? UserName { get; set; }
        public string? UserEmail { get; set; }
        public string? InvitedBy { get; set; }

        public MRegistrationParam()
        {
        }
    }
}
