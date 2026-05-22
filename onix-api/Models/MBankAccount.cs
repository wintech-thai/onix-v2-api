using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using IndexAttribute = Microsoft.EntityFrameworkCore.IndexAttribute;
using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.Models
{
    [ExcludeFromCodeCoverage]
    [Table("BankAccounts")]

    [Index(nameof(OrgId))]
    [Index(nameof(AccountType))]
    [Index(nameof(AccountCategory))]
    public class MBankAccount
    {
        [Key]
        [Column("bank_account_id")]
        public Guid? Id { get; set; }

        [Column("org_id")]
        public string? OrgId { get; set; }

        [Column("bank_code")]
        public string? BankCode { get; set; } //เป็นรหัสธนาคารมาตรฐานตาม BOT กำหนด

        [Column("account_number")]
        public string? AccountNumber { get; set; }

        [Column("account_name")]
        public string? AccountName { get; set; }

        [Column("promptpay_id")]
        public string? PromptPayId { get; set; } //สำหรับบัญชีที่เป็น PromptPay PromptPayId จะเป็นหมายเลขโทรศัพท์หรือหมายเลขบัตรประชาชนที่ลงทะเบียนกับ PromptPay

        [Column("tags")]
        public string? Tags { get; set; }

        [Column("account_type")]
        public string? AccountType { get; set; } // Native, PromptPay

        [Column("account_category")]
        public string? AccountCategory { get; set; } // PayIn, PayOut

        [Column("account_level")]
        public string? AccountLevel { get; set; } // Global, Selected - ใช้ได้ทุก merchant หรือใช้ได้เฉพาะบาง merchant

        [Column("payin_min_amount")]
        public double? PayinMinAmount { get; set; }

        [Column("payin_max_amount")]
        public double? PayinMaxAmount { get; set; }

        [Column("payout_min_amount")]
        public double? PayoutMinAmount { get; set; }

        [Column("payout_max_amount")]
        public double? PayoutMaxAmount { get; set; }

        [Column("daily_quota")]
        public double? DailyQuota { get; set; } //ยอดเงินรวมสูงสุดที่สามารถทำธุรกรรมได้ในแต่ละวัน

        [Column("current_daily_payin_amount")]
        public double? CurrentDailyPayinAmount { get; set; } //ยอดเงินรวมที่ทำธุรกรรมไปแล้วในวันนั้น

        [Column("current_daily_payin_count")]
        public double? CurrentDailyPayinCount { get; set; } //จำนวนครั้งที่ทำธุรกรรม PayIn ไปแล้วในวันนั้น

        [Column("current_balance")]
        public double? CurrentBalance { get; set; } //ยอดเงินคงเหลือในบัญชี

        [Column("daily_payin_count_quota")]
        public double? DailyPayinCountQuota { get; set; } //จำนวนครั้งสูงสุดที่สามารถทำธุรกรรม PayIn ได้ในแต่ละวัน

        [Column("Status")]
        public string? Status { get; set; } //Active, Pending, Disabled

        //System fields
        [Column("created_date")]
        public DateTime? CreatedDate { get; set; }
        [Column("last_used_date")]
        public DateTime? LastUsedDate { get; set; }


        [NotMapped]
        public int? MerchantLinkCount { get; set; }

        [NotMapped]
        public decimal? CurrentWalletBalance { get; set; }

        public MBankAccount()
        {
            Id = Guid.NewGuid();
            CreatedDate = DateTime.UtcNow;
        }
    }
}
