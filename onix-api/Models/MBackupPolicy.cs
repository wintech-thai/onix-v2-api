using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.Models
{
    [ExcludeFromCodeCoverage]
    [NotMapped]
    public class MBackupPolicy
    {
        public string? StorageUrl { get; set; }
        public string? StorageKey { get; set; }
        public string? StorageSecret { get; set; }
        public string? Bucket { get; set; }
        public string? Path { get; set; }
        public string? FilePrefix { get; set; }
        public string? ScheduleInterval { get; set; } /* 4h, 8h, 12h, daily */
        public int ScheduleStartHour { get; set; }
        public bool IsEnabled { get; set; }
    }
}
