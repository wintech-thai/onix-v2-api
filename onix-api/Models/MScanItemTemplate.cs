using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using IndexAttribute = Microsoft.EntityFrameworkCore.IndexAttribute;
using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.Models
{
    [ExcludeFromCodeCoverage]
    [Table("ScanItemTemplates")]

    [Index(nameof(OrgId))]
    public class MScanItemTemplate
    {
        [Key]
        [Column("template_id")]
        public Guid? Id { get; set; }

        [Column("org_id")]
        public string? OrgId { get; set; }

        [Column("serial_prefix_digit")] /* 2 */
        public int? SerialPrefixDigit { get; set; }

        [Column("generator_count")] /* 100 */
        public int? GeneratorCount { get; set; }

        [Column("serial_digit")] /* 7 */
        public int? SerialDigit { get; set; }

        [Column("pin_digit")] /* 7 */
        public int? PinDigit { get; set; }

        [Column("url_template")]
        public string? UrlTemplate { get; set; }

        [Column("noti_email")]
        public string? NotificationEmail { get; set; }

        //System fields
        [Column("created_date")]
        public DateTime? CreatedDate { get; set; }

        public object? GetPropertyValue(string propertyName)
        {
            var prop = this.GetType().GetProperty(propertyName);
            if (prop == null) return null; // ถ้าไม่เจอ property

            return prop.GetValue(this);
        }

        public MScanItemTemplate()
        {
            Id = Guid.NewGuid();
            CreatedDate = DateTime.UtcNow;
        }
    }
}
