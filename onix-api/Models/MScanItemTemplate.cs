using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using IndexAttribute = Microsoft.EntityFrameworkCore.IndexAttribute;
using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.Models
{
    [ExcludeFromCodeCoverage]
    [Table("ScanItemTemplates")]

    [Index(nameof(OrgId))]
    [Index(nameof(TemplateName))]
    
    public class MScanItemTemplate
    {
        [Key]
        [Column("template_id")]
        public Guid? Id { get; set; }

        [Column("org_id")]
        public string? OrgId { get; set; }

        [Column("template_name")]
        public string? TemplateName { get; set; }

        [Column("description")]
        public string? Description { get; set; }

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

        [Column("tags")]
        public string? Tags { get; set; }

        [Column("is_default")] 
        public string? IsDefault { get; set; } /* YES or NO, ถ้า YES เป็นตัว default จะมี YES ได้แค่ 1 อัน เท่านั้น */


        //System fields
        [Column("created_date")]
        public DateTime? CreatedDate { get; set; }

        public string GetPropertyValue(string propertyName, string defaultValue)
        {
            var prop = GetType().GetProperty(propertyName);
            if (prop == null) return "";

            var value = prop.GetValue(this);
            if (value == null) return defaultValue;

            return value.ToString()!;
        }

        public MScanItemTemplate()
        {
            Id = Guid.NewGuid();
            CreatedDate = DateTime.UtcNow;
        }
    }
}
