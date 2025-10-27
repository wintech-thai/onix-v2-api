using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using IndexAttribute = Microsoft.EntityFrameworkCore.IndexAttribute;
using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.Models
{
    [ExcludeFromCodeCoverage]
    [Table("Entities")]

    [Index(nameof(OrgId))]
    public class MEntity
    {
        [Key]
        [Column("entity_id")]
        public Guid? Id { get; set; }

        [Column("org_id")]
        public string? OrgId { get; set; }

        [Column("code")]
        public string? Code { get; set; }

        [Column("name")]
        public string? Name { get; set; }

        [Column("tags")]
        public string? Tags { get; set; }

        [Column("entity_type")] /* 1=Customer, 2=Supplier */
        public int? EntityType { get; set; }

        [Column("entity_category")] /* 1=Personal, 2=Company */
        public int? EntityCategory { get; set; }

        [Column("credit_term_day")] 
        public int? CreditTermDay { get; set; }

        [Column("credit_amount")] 
        public double? CreditAmount { get; set; }

        [Column("tax_id")]
        public string? TaxId { get; set; }

        [Column("national_card_id")]
        public string? NationalCardId { get; set; }

        [Column("primary_email")]
        public string? PrimaryEmail { get; set; }

        [Column("secondary_email")]
        public string? SecondaryEmail { get; set; }

        [Column("content")]
        public string? Content { get; set; }
        
        //Navigation Properties
        public ICollection<MPricingPlan> PricingPlans { get; set; } = new List<MPricingPlan>();

        //System fields
        [Column("created_date")]
        public DateTime? CreatedDate { get; set; }

        [Column("updated_date")]
        public DateTime? UpdatedDate { get; set; }

        public MEntity()
        {
            Id = Guid.NewGuid();
            CreatedDate = DateTime.UtcNow;
            UpdatedDate = DateTime.UtcNow;
        }
    }
}
