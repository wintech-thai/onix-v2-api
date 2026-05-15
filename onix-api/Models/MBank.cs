using System.ComponentModel.DataAnnotations.Schema;

namespace Its.Onix.Api.Models
{
    public class MBank
    {
        public string? BankCode { get; set; } //เป็นรหัสธนาคารมาตรฐานตาม BOT กำหนด
        public string? BankShortName { get; set; } //เป็นชื่อธนาคารมาตรฐานตาม BOT กำหนด

        public string? BankNameEng { get; set; } //เป็นชื่อธนาคารมาตรฐานตาม BOT กำหนด
        public string? BankNameTh { get; set; } //เป็นชื่อธนาคารมาตรฐานตาม BOT กำหนด
        public bool QrSupportFlag { get; set; } //สนับสนุน QR scan หรือยัง
        public string Type { get; set; } //Native & Promptpay

        public MBank()
        {
            QrSupportFlag = false;
            Type = "Native";
        }
    }
}
