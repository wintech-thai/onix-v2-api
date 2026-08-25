using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using IndexAttribute = Microsoft.EntityFrameworkCore.IndexAttribute;
using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.Models
{
    [ExcludeFromCodeCoverage]
    [Table("CurrencyAccountMerchants")]

    [Index(nameof(OrgId))]
    [Index(nameof(Currency))]
    [Index(nameof(MerchantId))]
    [Index(nameof(CurrencyAccountId))]

    public class MCurrencyAccountMerchant
    {
        [Key]
        [Column("account_merchant_id")]
        public Guid? Id { get; set; }

        [Column("org_id")]
        public string? OrgId { get; set; }

        [Column("currency_account_id")]
        public string? CurrencyAccountId { get; set; }

        [Column("currency")]
        public string? Currency { get; set; }

        [Column("currency_category")]
        public string? CurrencyCategory { get; set; }

        public string? MerchantId { get; set; }

        [Column("account_category")]
        public string? AccountCategory { get; set; } // PayIn, PayOut

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
        public string? AccountLevel { get; set; } // Global, Selected - ใช้ได้ทุก merchant หรือใช้ได้เฉพาะบาง merchant

        [NotMapped]
        public double? TxMinAmount { get; set; }

        [NotMapped]
        public double? TxMaxAmount { get; set; }

        [NotMapped]
        public double? DailyTxAmountLimit { get; set; } //ยอดเงินรวมสูงสุดที่สามารถทำธุรกรรมได้ในแต่ละวัน

        [NotMapped]
        public double? DailyTxCountLimit { get; set; } //จำนวนครั้งสูงสุดที่สามารถทำธุรกรรม PayIn ได้ในแต่ละวัน


        [NotMapped]
        public double? CurrentDailyPayinAmount { get; set; } //ยอดเงินรวมที่ทำธุรกรรมไปแล้วในวันนั้น

        [NotMapped]
        public double? CurrentDailyPayinCount { get; set; } //จำนวนครั้งที่ทำธุรกรรม PayIn ไปแล้วในวันนั้น

        [NotMapped]
        public double? CurrentBalance { get; set; } //ยอดเงินคงเหลือในบัญชี

        [NotMapped]
        public string? BankAccountStatus { get; set; } //Active, Pending, Disabled

        [NotMapped]
        public int? MerchantCount { get; set; }

        [NotMapped]
        public int? BankAccountCount { get; set; }

        public MCurrencyAccountMerchant()
        {
            Id = Guid.NewGuid();
            CreatedDate = DateTime.UtcNow;
        }
    }
}
