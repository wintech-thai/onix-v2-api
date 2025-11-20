namespace Its.Onix.Api.Models
{
    public class PointRuleInput
    {
        public string? ProductCode { get; set; }
        public string? ProductTags { get; set; }
        public int? ProductQuantity { get; set; }
        public double? PaidAmount { get; set; }
        public DateTime? CurrentDate { get; set; }

        public string? RuleDefinition { get; set; }

        public PointRuleInput()
        {
            ProductQuantity = 1;
        }
    }
}
