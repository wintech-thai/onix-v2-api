using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using IndexAttribute = Microsoft.EntityFrameworkCore.IndexAttribute;
using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.Models
{
    [ExcludeFromCodeCoverage]
    [Table("Organizations")]
    [Index(nameof(OrgCustomId), IsUnique = true)]
    public class MOrganization
    {
        [Key]
        [Column("org_id")]
        public Guid? OrgId { get; set; }
    
        [Column("org_custom_id")]
        public string? OrgCustomId { get; set; }

        [Column("org_name")]
        public string? OrgName { get; set; }

        [Column("org_description")]
        public string? OrgDescription { get; set; }

        [Column("org_type")]
        public string? OrgType { get; set; } // PLEASE-SCAN, PLEASE-PROTECT, PELASE-PAYMENT

        [Column("tags")]
        public string? Tags { get; set; } // Comma separated string

        [Column("addresses")]
        public string? Addresses { get; set; } // JSON string

        [Column("channels")]
        public string? Channels { get; set; } // JSON string

        [Column("logo_image_path")]
        public string? LogoImagePath { get; set; } // JSON string

        [Column("logo_image_base64")]
        public string? LogoImageBase64 { get; set; }

        [Column("org_name_th")]
        public string? OrgNameTh { get; set; }

        [Column("org_name_en")]
        public string? OrgNameEn { get; set; }

        [Column("contact_name_th")]
        public string? ContactNameTh { get; set; }

        [Column("contact_name_en")]
        public string? ContactNameEn { get; set; }

        [Column("address_th")]
        public string? AddressTh { get; set; }

        [Column("address_en")]
        public string? AddressEn { get; set; }

        [Column("phone")]
        public string? Phone { get; set; }

        [Column("fax")]
        public string? Fax { get; set; }

        [Column("email")]
        public string? Email { get; set; }

        [Column("tax_id")]
        public string? TaxId { get; set; }

        [Column("website")]
        public string? Website { get; set; }

        [Column("Status")]
        public string? Status { get; set; } //Active, Pending, Disabled


        [NotMapped]
        public MMerchant? Merchant { get; set; }

        [NotMapped]
        public List<NameValue> AddressesArray { get; set; }
        [NotMapped]
        public List<NameValue> ChannelsArray { get; set; }
        [NotMapped]
        public string? LogoImageUrl { get; set; }

        [Column("org_created_date")]
        public DateTime? OrgCreatedDate { get; set; }

        public MOrganization()
        {
            OrgId = Guid.NewGuid();
            OrgCreatedDate = DateTime.UtcNow;

            AddressesArray = [];
            ChannelsArray = [];
        }
    }
}
