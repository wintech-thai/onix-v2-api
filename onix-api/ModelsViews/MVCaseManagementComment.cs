using System.Diagnostics.CodeAnalysis;
using Its.Onix.Api.Models;

namespace Its.Onix.Api.ModelsViews
{
    [ExcludeFromCodeCoverage]
    public class MVCaseManagementComment
    {
        public string? Status { get; set; }
        public string? Description { get; set; }
        public MCaseManagementComment? Comment { get; set; }
        public List<MCaseManagementComment>? Comments { get; set; }
    }
}
