namespace Its.Onix.Api.Models
{
    public class PointTriggerInput
    {
        public PointRuleInput? PointRuleInput { get; set; }
        public string? WalletId { get; set; }
        public string? EventTriggered { get; set; }

        public PointTriggerInput()
        {
        }
    }
}
