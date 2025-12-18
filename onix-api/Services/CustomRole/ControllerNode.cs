namespace Its.Onix.Api.Models
{
    public class ControllerNode
    {
        public string ControllerName { get; set; }
        public List<ApiNode> ApiPermissions { get; set; }

        public ControllerNode()
        {
            ControllerName = "";
            ApiPermissions = [];
        }
    }
}
