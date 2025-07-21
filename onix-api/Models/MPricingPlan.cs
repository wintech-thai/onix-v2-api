using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using IndexAttribute = Microsoft.EntityFrameworkCore.IndexAttribute;
using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.Models
{
    [ExcludeFromCodeCoverage]
    [Table("PricingPlans")]

    [Index(nameof(OrgId))]
    public class MPricingPlan
    {
        [Key]
        [Column("plan_id")]
        public Guid? Id { get; set; }

        [Column("org_id")]
        public string? OrgId { get; set; }

        [Column("code")]
        public string? Code { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        [Column("tags")]
        public string? Tags { get; set; }

        [Column("cycle_id")]
        [ForeignKey("Cycle")]
        public Guid? CycleId { get; set; }
        public MCycle? Cycle { get; set; }  // <== Navigation property

        [Column("customer_id")]
        [ForeignKey("Customer")]
        public Guid? CustomerId { get; set; }
        public MEntity? Customer { get; set; }  // <== Navigation property

        [Column("status")] /* 1=Active, 2=Disable */
        public int? Status { get; set; }

        [Column("priority")]
        public int? Priority { get; set; }

        [Column("start_date")]
        public DateTime? StargDate { get; set; }

        [Column("end_date")]
        public DateTime? EndDate { get; set; }


        //System fields
        [Column("created_date")]
        public DateTime? CreatedDate { get; set; }

        [Column("updated_date")]
        public DateTime? UpdatedDate { get; set; }

        public MPricingPlan()
        {
            Id = Guid.NewGuid();
            CreatedDate = DateTime.UtcNow;
            UpdatedDate = DateTime.UtcNow;
        }
    }
}
