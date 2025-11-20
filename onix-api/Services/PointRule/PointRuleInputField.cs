namespace Its.Onix.Api.Models
{
    public class PointRuleInputField
    {
        public string FieldName { get; set; }
        public string DefaultValue { get; set; }
        public string FieldType { get; set; }

        public PointRuleInputField()
        {
            FieldName = "";
            DefaultValue = "";
            FieldType = "string";
        }
    }
}
