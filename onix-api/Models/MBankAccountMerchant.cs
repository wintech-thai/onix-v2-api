using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using IndexAttribute = Microsoft.EntityFrameworkCore.IndexAttribute;
using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.Models
{
    [ExcludeFromCodeCoverage]
    [Table("BankAccountMerchants")]

    [Index(nameof(OrgId))]
    public class MBankAccountMerchant
    {
        [Key]
        [Column("bankaccount_merchant_id")]
        public Guid? Id { get; set; }

        [Column("org_id")]
        public string? OrgId { get; set; }

        [Column("bank_account_id")]
        public string? BankAccountId { get; set; }

        [Column("merchant_id")]
        public string? MerchantId { get; set; }

        //System fields
        [Column("created_date")]
        public DateTime? CreatedDate { get; set; }


        // Merchant Fields
        [NotMapped]
        public string? MerchantCode { get; set; }
        [NotMapped]
        public string? MerchantName { get; set; }
        [NotMapped]
        public string? MerchantStatus { get; set; }


        //Bank Account Fields
        [NotMapped]
        public string? BankCode { get; set; } //เป็นรหัสธนาคารมาตรฐานตาม BOT กำหนด

        [NotMapped]
        public string? AccountNumber { get; set; }

        [NotMapped]
        public string? AccountName { get; set; }

        [NotMapped]
        public string? PromptPayId { get; set; } //สำหรับบัญชีที่เป็น PromptPay PromptPayId จะเป็นหมายเลขโทรศัพท์หรือหมายเลขบัตรประชาชนที่ลงทะเบียนกับ PromptPay

        [NotMapped]
        public string? AccountType { get; set; } // Native, PromptPay

        [NotMapped]
        public string? AccountCategory { get; set; } // PayIn, PayOut

        [NotMapped]
        public string? AccountLevel { get; set; } // Global, Selected - ใช้ได้ทุก merchant หรือใช้ได้เฉพาะบาง merchant

        [NotMapped]
        public double? PayinMinAmount { get; set; }

        [NotMapped]
        public double? PayinMaxAmount { get; set; }

        [NotMapped]
        public double? PayoutMinAmount { get; set; }

        [NotMapped]
        public double? PayoutMaxAmount { get; set; }

        [NotMapped]
        public double? DailyQuota { get; set; } //ยอดเงินรวมสูงสุดที่สามารถทำธุรกรรมได้ในแต่ละวัน

        [NotMapped]
        public double? CurrentDailyPayinAmount { get; set; } //ยอดเงินรวมที่ทำธุรกรรมไปแล้วในวันนั้น

        [NotMapped]
        public double? CurrentDailyPayinCount { get; set; } //จำนวนครั้งที่ทำธุรกรรม PayIn ไปแล้วในวันนั้น

        [NotMapped]
        public double? CurrentBalance { get; set; } //ยอดเงินคงเหลือในบัญชี

        [NotMapped]
        public double? DailyPayinCountQuota { get; set; } //จำนวนครั้งสูงสุดที่สามารถทำธุรกรรม PayIn ได้ในแต่ละวัน

        [NotMapped]
        public string? BankAccountStatus { get; set; } //Active, Pending, Disabled

        [NotMapped]
        public int? MerchantCount { get; set; }

        [NotMapped]
        public int? BankAccountCount { get; set; }

        public MBankAccountMerchant()
        {
            Id = Guid.NewGuid();
            CreatedDate = DateTime.UtcNow;
        }
    }
}
