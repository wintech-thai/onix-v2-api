using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.Utils
{
    [ExcludeFromCodeCoverage]
    public class PathComponent
    {
        public string OrgId {get; set;}
        public string ControllerName {get; set;}
        public string ApiName {get; set;}

        public PathComponent()
        {
            ApiName = "";
            ControllerName = "";
            OrgId = "";
        }
    }
}
