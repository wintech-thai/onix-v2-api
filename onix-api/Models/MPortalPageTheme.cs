using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using IndexAttribute = Microsoft.EntityFrameworkCore.IndexAttribute;
using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.Models
{
    [ExcludeFromCodeCoverage]
    [Table("PortalPageThemes")]
    [Index(nameof(OrgId))]
    [Index(nameof(OrgId), nameof(ThemeName), IsUnique = true, Name = "PortalPageTheme_Org_Name_Unique")]
    public class MPortalPageTheme
    {
        [Key]
        [Column("theme_id")]
        public Guid? ThemeId { get; set; }

        //ตามสเปค theme เป็นของ global OrgId เสมอ ไม่ผูกกับ tenant ไหน
        [Column("org_id")]
        public string? OrgId { get; set; }

        [Column("theme_name")]
        public string? ThemeName { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        [Column("auth_type")]
        public string? AuthType { get; set; } /* Native, Google - ล็อกตั้งแต่ตอนสร้าง แก้ทีหลังไม่ได้ */

        [Column("language")]
        public string? Language { get; set; } /* Thai, English */

        [Column("variables_json")]
        public string? VariablesJson { get; set; } /* JSON array ของ {Name, Value} ชื่อไม่ซ้ำกันในชุดเดียวกัน */

        [Column("html_template", TypeName = "text")]
        public string? HtmlTemplate { get; set; }

        [Column("created_date")]
        public DateTime? CreatedDate { get; set; }

        //นับจาก PortalPages ที่ผูก ThemeId นี้อยู่ - ไม่ denormalize มาเก็บ คำนวณสดตอน query เสมอ
        [NotMapped]
        public int UseCount { get; set; }

        //serialize/deserialize กับ VariablesJson ที่ฝั่ง Service เสมอ ไม่ได้ map ตรงๆ กับ DB column
        [NotMapped]
        public List<MPortalPageThemeVariable>? Variables { get; set; }

        public MPortalPageTheme()
        {
            ThemeId = Guid.NewGuid();
            CreatedDate = DateTime.UtcNow;
            Variables = [];
        }
    }

    [ExcludeFromCodeCoverage]
    public class MPortalPageThemeVariable
    {
        public string? Name { get; set; }
        public string? Value { get; set; }
    }
}
