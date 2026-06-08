using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using IndexAttribute = Microsoft.EntityFrameworkCore.IndexAttribute;
using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.Models
{
    [ExcludeFromCodeCoverage]
    [Table("NotiAlertChannels")]
    [Index(nameof(ChannelName), IsUnique = false)]
    [Index(nameof(Description), IsUnique = false)]
    [Index(nameof(Tags), IsUnique = false)]
    [Index(nameof(Type), IsUnique = false)]
    [Index(nameof(Status), IsUnique = false)]

    public class MNotiChannel
    {
        [Key]
        [Column("noti_channel_id")]
        public Guid? Id { get; set; }
    
        [Column("org_id")]
        public string? OrgId { get; set; }

        [Column("channel_name")]
        public string? ChannelName { get; set; }

        [Column("channel_description")]
        public string? Description { get; set; }

        [Column("tags")]
        public string? Tags { get; set; }

        [Column("type")]
        public string? Type { get; set; } /* Discord, Telegram, Slack, Email, LINE, Webhook */

        [Column("status")]
        public string? Status { get; set; } /* Enabled, Disabled */

        [Column("events_matched")]
        public string? EventsMatched { get; set; } /* JSON array of events name */


        [Column("message_template")]
        public string? MessageTemplate { get; set; } /* JSON object containing message template */



        [Column("discord_webhook_url")]
        public string? DiscordWebhookUrl { get; set; }

        [Column("telegram_webhook_url")]
        public string? TelegramWebhookUrl { get; set; }

        [Column("telegram_chat_id")]
        public string? TelegramChatId { get; set; }

        [Column("created_date")]
        public DateTime? CreatedDate { get; set; }


        [NotMapped]
        public IEnumerable<string>? EventTypes { get; set; }

        public MNotiChannel()
        {
            Id = Guid.NewGuid();
            CreatedDate = DateTime.UtcNow;
        }
    }
}
