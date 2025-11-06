using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using IndexAttribute = Microsoft.EntityFrameworkCore.IndexAttribute;
using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.Models
{
    [ExcludeFromCodeCoverage]
    [Table("Limits")]

    [Index(nameof(OrgId))]
    [Index(nameof(StatCode))]

    public class MLimit
    {
        [Key]
        [Column("limit_id")]
        public Guid? Id { get; set; }

        [Column("org_id")]
        public string? OrgId { get; set; }

        [Column("stat_code")] //ค่าเดียวกันกับใน table Stats 
        public string? StatCode { get; set; }

        [Column("limit")]
        public long? Limit { get; set; }

        //System fields
        [Column("created_date")]
        public DateTime? CreatedDate { get; set; }

        public MLimit()
        {
            Id = Guid.NewGuid();
            CreatedDate = DateTime.UtcNow;
        }
    }
}
