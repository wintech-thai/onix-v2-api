namespace Its.Onix.Api.ModelsViews
{
    //เก็บใน Redis ระหว่างรอ Google redirect กลับมา
    public class MVGoogleOAuthState
    {
        public string? OrgId { get; set; }
        public string? PortalSessionId { get; set; }
        public string? ClientId { get; set; }
        public string? ClientSecret { get; set; }
        public string? RedirectUri { get; set; } /* ต้องตรงกับตอน authorize */
        public string? Nonce { get; set; }
    }
}
