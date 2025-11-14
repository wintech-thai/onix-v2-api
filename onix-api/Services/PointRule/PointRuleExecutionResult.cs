namespace Its.Onix.Api.Models
{
    public class PointRuleExecutionResult
    {
        public bool IsMatch { get; set; }
        public MPointRule? RuleMatch { get; set; }
        public string? ExecutionResult;

        public PointRuleExecutionResult()
        {
        }
    }
}
