using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.Models
{
    [ExcludeFromCodeCoverage]
    [Table("MSystemVariables")]
    public class MSystemVariable
    {
        [Key]
        [Column("variable_id")]
        public Guid? VariableId { get; set; }
        
        [Column("org_id")]
        public string? OrgId { get; set; }

        [Column("varaible_name")]
        public string? VariableName { get; set; }

        [Column("varaible_value")]
        public string? VariableValue { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        [Column("created_date")]
        public DateTime? CreatedDate { get; set; }

        public MSystemVariable()
        {
            VariableId = Guid.NewGuid();
            CreatedDate = DateTime.UtcNow;
        }
    }
}
