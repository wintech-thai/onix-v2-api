using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using IndexAttribute = Microsoft.EntityFrameworkCore.IndexAttribute;
using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.Models
{
    [ExcludeFromCodeCoverage]
    [Table("AuditLogs")]

    [Index(nameof(OrgId))]
    public class MAuditLog
    {
        [Key]
        [Column("log_id")]
        public Guid? Id { get; set; }

        [Column("org_id")]
        public string? OrgId { get; set; }

        [Column("http_method")]
        public string? HttpMethod { get; set; }

        [Column("status_code")]
        public int? StatusCode { get; set; }

        [Column("path")]
        public string? Path { get; set; }

        [Column("query_string")]
        public string? QueryString { get; set; }

        [Column("user_agent")]
        public string? UserAgent { get; set; }

        [Column("host")]
        public string? Host { get; set; }

        [Column("scheme")]
        public string? Scheme { get; set; }

        [Column("client_ip")]
        public string? ClientIp { get; set; }

        [Column("client_ip_cf")]
        public string? CfClientIp { get; set; }

        [Column("environment")]
        public string? Environment { get; set; }

        [Column("custom_status")]
        public string? CustomStatus { get; set; }

        [Column("custom_desc")]
        public string? CustomDesc { get; set; }

        [Column("request_size")]
        public long? RequestSize { get; set; }

        [Column("response_size")]
        public long? ResponseSize { get; set; }

        [Column("latency_ms")]
        public long? LatencyMs { get; set; }

        [Column("role")]
        public string? Role { get; set; }

        [Column("identity_type")]
        public string? IdentityType { get; set; }

        [Column("user_id")]
        public string? UserId { get; set; }

        [Column("user_name")]
        public string? UserName { get; set; }

        //System fields
        [Column("created_date")]
        public DateTime? CreatedDate { get; set; }

        public MAuditLog()
        {
            Id = Guid.NewGuid();
            CreatedDate = DateTime.UtcNow;
        }
    }
}
