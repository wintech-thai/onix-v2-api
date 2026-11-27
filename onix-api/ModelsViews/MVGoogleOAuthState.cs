namespace Its.Onix.Api.ModelsViews
{
    //เก็บใน Redis ชั่วคราวระหว่างรอ Google redirect กลับมา (key = GoogleOAuth:{OTP}, TTL 1 ชม.)
    public class MVGoogleOAuthState
    {
        public string? OrgId { get; set; }
        public string? PortalSessionId { get; set; }
        public string? ClientId { get; set; }
        public string? ClientSecret { get; set; }
        public string? RedirectUri { get; set; } /* ต้องเป็นค่าเดียวกับที่ส่งไปตอน authorize ใช้ตอนแลก token ด้วย */
        public string? Nonce { get; set; }
    }
}
