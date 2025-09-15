using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.Models
{
    [ExcludeFromCodeCoverage]
    public class MImage
    {
        public Guid? Id { get; set; }

        public string? OrgId { get; set; }

        public Guid? ItemId { get; set; }

        public string? ImagePath { get; set; }

        public string? ImageUrl { get; set; }

        public string? Narative { get; set; }

        public string? Tags { get; set; }

        public int? Category { get; set; }

        public MImage()
        {
            Id = Guid.NewGuid();
        }
    }
}
