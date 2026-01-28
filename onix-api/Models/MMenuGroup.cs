namespace Its.Onix.Api.Models
{
    public class MMenuGroup
    {
        public string? GroupName { get; set; }
        public bool IsVisible { get; set; } = true;
        public List<MMenuItem> MenuItems { get; set; } = [];
    }
}
