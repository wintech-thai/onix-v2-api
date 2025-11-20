namespace Its.Onix.Api.Models
{
    public class PointRuleExecutionResult
    {
        public string Status { get; set; }
        public string Description { get; set; }

        public bool IsMatch { get; set; }
        public MPointRule? RuleMatch { get; set; }
        public string? ExecutionResult { get; set; }

        public List<string> Messages { get; set; }

        public PointRuleExecutionResult()
        {
            Status = "";
            Description = "";
            Messages = [];
        }
    }
}
