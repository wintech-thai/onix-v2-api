using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.Models
{
    [ExcludeFromCodeCoverage]
    public class MItemProperties
    {
        public string? DimentionUnit { get; set; }
        public string? WeightUnit { get; set; }

        public double? Width { get; set; }

        public double? Height { get; set; }

        public double? Weight { get; set; }


        public MItemProperties()
        {
            DimentionUnit = "cm";
            WeightUnit = "gram";
        }
    }
}
