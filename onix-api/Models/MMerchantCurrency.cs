using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using IndexAttribute = Microsoft.EntityFrameworkCore.IndexAttribute;
using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.Models
{
    [ExcludeFromCodeCoverage]
    [Table("MerchantCurrencies")]

    [Index(nameof(OrgId))]
    [Index(nameof(MerchantId))]
    [Index(nameof(Currency))]
    [Index(nameof(CurrencyCategory))]

    public class MMerchantCurrency
    {
        [Key]
        [Column("merchant_policy_id")]
        public Guid? Id { get; set; }

        [Column("org_id")]
        public string? OrgId { get; set; }

        [Column("MerchantId")]
        public string? MerchantId { get; set; }

        [Column("currency")]
        public string? Currency { get; set; } /* THB, USD, USDT */

        [Column("currency_name")]
        public string? CurrencyName { get; set; }


        [Column("currency_category")]
        public string? CurrencyCategory { get; set; } /* FIAT, CRYPTO */

        [Column("is_default_currency")]
        public bool IsDefaultCurrency { get; set; } /* เป็น true ได้อันเดียวต่อ 1 merchant, ใช้แสดง default */

        [Column("status")]
        public string? Status { get; set; } /* Active, Disabled */

        [Column("wallet_id")]
        public string? WalletId { get; set; } /* Active, Disabled */


        [Column("payin_fee_pct")]
        public double? PayinFeePct { get; set; }

        [Column("payin_min_amount")]
        public double? PayinMinAmount { get; set; }

        [Column("payin_max_amount")]
        public double? PayinMaxAmount { get; set; }

        [Column("pay_indiscard_cent")]
        public bool PayinDiscardCent { get; set; } //หักเศษสตางค์มาเป็น ค่าธรรมเนียม

        [Column("payin_include_global_bank_account")]
        public bool PayinIncludeGlobalBankAccount { get; set; } //true = สามารถใช้ global pay-in bank account ได้ตอนสร้าง QR

        [Column("payin_whitelist_bank_account_names")]
        public string? PayinWhitelistBankAccountNames { get; set; } //serialize มาจาก List<string> WhitelistBankAccountNamesArr ห้าม return ออกไปตรง ๆ

        [Column("payin_random_decimal")]
        public bool? PayinRandomDecimal { get; set; }

        [Column("payin_daily_tx_amount_limit")]
        public decimal? PayinDailyTxAmountLimit { get; set; }

        [Column("payin_daily_tx_count_limit")]
        public decimal? PayinDailyTxCountLimit { get; set; }

        [Column("payin_expire_minute")]
        public int? PayinExpireMinute { get; set; }



        [Column("payout_fee_pct")]
        public double? PayoutFeePct { get; set; }

        [Column("payout_min_amount")]
        public double? PayoutMinAmount { get; set; }

        [Column("payout_max_amount")]
        public double? PayoutMaxAmount { get; set; }


        //System fields
        [Column("created_date")]
        public DateTime? CreatedDate { get; set; }

        public MMerchantCurrency()
        {
            Id = Guid.NewGuid();
            CreatedDate = DateTime.UtcNow;
        }
    }
}
