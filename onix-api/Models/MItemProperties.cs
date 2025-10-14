using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.Models
{
    [ExcludeFromCodeCoverage]
    public class MItemProperties
    {
        public string? DimensionUnit { get; set; }
        public string? WeightUnit { get; set; }
        public string? Category { get; set; }
        public string? SupplierUrl { get; set; }
        public string? ProductUrl { get; set; }

        public double? Width { get; set; }

        public double? Height { get; set; }

        public double? Weight { get; set; }


        public MItemProperties()
        {
            //DimensionUnit = "cm";
            //WeightUnit = "gram";
        }
    }
}
