using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using IndexAttribute = Microsoft.EntityFrameworkCore.IndexAttribute;
using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.Models
{
    [ExcludeFromCodeCoverage]
    [Table("Variables")]
    public class MVariable
    {
        [Key]
        [Column("variable_id")]
        public Guid? VariableId { get; set; }

        [Column("variable_name")]
        public string? VariableName { get; set; }

        [Column("variable_type")]
        public string? VariableType { get; set; }
    
        [Column("variable_desc")]
        public string? VariableDesc { get; set; }

        [Column("variable_value")]
        public string? VariableValue { get; set; }

        public MVariable()
        {
            VariableId = Guid.NewGuid();
        }
    }
}
