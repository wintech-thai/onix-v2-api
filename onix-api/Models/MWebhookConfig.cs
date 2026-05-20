using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.Models
{
    [ExcludeFromCodeCoverage]
    [Table("WebhookConfigs")]

    public class MWebhookConfig
    {
        [Key]
        [Column("webhook_id")]
        public Guid? WebhookId { get; set; }

        [Column("org_id")]
        public string? OrgId { get; set; }

        // merchant owner
        [Column("merchant_id")]
        public Guid? MerchantId { get; set; }

        // payment.success
        [Column("event_name")]
        public string? EventName { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        // destination url
        [Column("endpoint_url")]
        public string? EndpointUrl { get; set; }

        // POST / PUT
        [Column("http_method")]
        public string? HttpMethod { get; set; }

        // enable / disable
        [Column("is_active")]
        public bool? IsActive { get; set; }

        // HMAC secret
        [Column("secret_key")]
        public string? SecretKey { get; set; }

        // HMAC-SHA256
        [Column("signature_algorithm")]
        public string? SignatureAlgorithm { get; set; }

        // custom headers json
        [Column("headers_definition")]
        public string? HeadersDefinition { get; set; }

        // timeout in seconds
        [Column("timeout_sec")]
        public int? TimeoutSec { get; set; }

        // retry count
        [Column("max_retry_count")]
        public int? MaxRetryCount { get; set; }

        // retry interval seconds
        [Column("retry_interval_sec")]
        public int? RetryIntervalSec { get; set; }

        // optional payload version
        [Column("payload_version")]
        public string? PayloadVersion { get; set; }

        // last test/send status
        [Column("last_status")]
        public string? LastStatus { get; set; }

        [Column("last_called_date")]
        public DateTime? LastCalledDate { get; set; }


        [Column("created_date")]
        public DateTime? CreatedDate { get; set; }


        [NotMapped]
        public Dictionary<string, string> Headers { get; set; }

        public MWebhookConfig()
        {
            EventName = "";
            Description = "";
            EndpointUrl = "";
            HttpMethod = "POST";
            IsActive = true;
            SecretKey = "";
            SignatureAlgorithm = "HMAC-SHA256";
            HeadersDefinition = "";
            Headers = [];
            TimeoutSec = 10;
            MaxRetryCount = 5;
            RetryIntervalSec = 30;
            PayloadVersion = "v1";
            LastStatus = "";
            CreatedDate = DateTime.UtcNow;
        }
    }
}
