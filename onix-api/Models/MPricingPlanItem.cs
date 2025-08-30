using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using IndexAttribute = Microsoft.EntityFrameworkCore.IndexAttribute;
using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.Models
{
    [ExcludeFromCodeCoverage]
    [Table("PricingPlanItems")]

    [Index(nameof(OrgId))]
    public class MPricingPlanItem
    {
        [Key]
        [Column("pricing_item_id")]
        public Guid? Id { get; set; }

        [Column("org_id")]
        public string? OrgId { get; set; }


        [Column("pricing_plan_id")]
        [ForeignKey("PricingPlan")]
        public Guid? PricingPlanId { get; set; }
        public MPricingPlan? PricingPlan { get; set; }  // <== Navigation property


        [Column("item_id")]
        //[ForeignKey("Item")]
        public Guid? ItemId { get; set; }
        public MItem? Item { get; set; }  // <== Navigation property

        [Column("rate_type")] /* 1=Flat, 2=Tier, 3=Step */
        public int? RateType { get; set; }

        [Column("flate_rate")] 
        public double? FlateRate { get; set; }

        [Column("rate_definition")]
        public string? RateDefinition { get; set; }


        //System fields
        [Column("created_date")]
        public DateTime? CreatedDate { get; set; }

        [Column("updated_date")]
        public DateTime? UpdatedDate { get; set; }

        public MPricingPlanItem()
        {
            Id = Guid.NewGuid();
            CreatedDate = DateTime.UtcNow;
            UpdatedDate = DateTime.UtcNow;
        }
    }
}
