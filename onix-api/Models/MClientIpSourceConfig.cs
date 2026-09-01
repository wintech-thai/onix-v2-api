using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.Models
{
    [ExcludeFromCodeCoverage]
    [NotMapped]
    public class MClientIpSourceConfig
    {
        public string? SourceType { get; set; } /* Native, Header */
        public string? HeaderName { get; set; }
        public int? HeaderIndex { get; set; }
    }
}
