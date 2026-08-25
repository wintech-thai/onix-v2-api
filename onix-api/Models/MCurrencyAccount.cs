using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using IndexAttribute = Microsoft.EntityFrameworkCore.IndexAttribute;
using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.Models
{
    [ExcludeFromCodeCoverage]
    [Table("CurrencyAccounts")]

    [Index(nameof(OrgId))]
    [Index(nameof(Currency))]
    [Index(nameof(CurrencyCategory))]
    [Index(nameof(AccountType))]
    [Index(nameof(CryptoWalletId))]
    [Index(nameof(CryptoWalletNetwork))]
    [Index(nameof(AccountKycName))]

    [Index(nameof(BankCode))]
    [Index(nameof(BankAccountNo))]
    [Index(nameof(BankAccountName))]
    [Index(nameof(CryptoExtendedPublicKey))]

    public class MCurrencyAccount
    {
        [Key]
        [Column("currency_account_id")]
        public Guid? Id { get; set; }

        [Column("org_id")]
        public string? OrgId { get; set; }

        //ข้อมูลทั่ว ๆ ไป
        [Column("currency")]
        public string? Currency { get; set; } //THB, USD, USDT, KAS

        [Column("currency_name")]
        public string? CurrencyName { get; set; }

        [Column("currency_category")]
        public string? CurrencyCategory { get; set; } //FIAT, CRYPTO

        [Column("account_kyc_name")]
        public string? AccountKycName { get; set; } //ใช้สำหรับเอาไปทำว่าเป็นของใคร KYC

        [Column("account_kyc_id")]
        public string? AccountKycId { get; set; } //เลขบัตรประชาชน

        [Column("account_kyc_email")]
        public string? AccountKycEmail { get; set; }

        [Column("account_kyc_phone")]
        public string? AccountKycPhone { get; set; }


        [Column("tags")]
        public string? Tags { get; set; }

        [Column("account_type")]
        public string? AccountType { get; set; } //PayIn, PayOut, Transit

        [Column("account_level")]
        public string? AccountLevel { get; set; } // Global, Selected - ใช้ได้ทุก merchant หรือใช้ได้เฉพาะบาง merchant

        [Column("Status")]
        public string? Status { get; set; } //Active, Disabled


        //ข้อมูลเกี่ยวกับ crypto
        [Column("crypto_wallet_id")]
        public string? CryptoWalletId { get; set; } //สำหรับ crypto

        [Column("crypto_wallet_network")]
        public string? CryptoWalletNetwork { get; set; } //TRON, ETHEREUM ...

        [Column("crypto_wallet_type")]
        public string? CryptoWalletType { get; set; } //HD

        [Column("crypto_derivation_path")]
        public string? CryptoDerivationPath { get; set; } //m/44'/195'/0'/0

        [Column("crypto_qr_scheme")]
        public string? CryptoQrScheme { get; set; } //TRON, ETHEREUM ...

        [Column("crypto_address_prefix")]
        public string? CryptoAddressPrefix { get; set; }

        [Column("crypto_token_contract")]
        public string? CryptoTokenContract { get; set; } 

        [Column("crypto_decimal")]
        public int CryptoDecimal { get; set; } //รองรับกี่ digit

        [Column("crypto_extended_public_key")]
        public string? CryptoExtendedPublicKey { get; set; } //EPK

        [Column("crypto_next_address_index")]
        public int CryptoNextAddressIndex { get; set; } //ต้องมีการบวกไปเรื่อย ๆ เมื่อสร้างใหม่, ควรทำ record locking การสร้าง address ใหม่สำหรับ wallet หนึ่ง ๆ

        [Column("crypto_address_branch")]
        public int CryptoAddressBranch { get; set; } //m/44'/195'/0'


        //ข้อมูลเกี่ยวกับบัญชีธนาคาร (fiat)
        [Column("bank_code")]
        public string? BankCode { get; set; }

        [Column("bank_name")]
        public string? BankName { get; set; }

        [Column("bank_account_name")]
        public string? BankAccountName { get; set; }

        [Column("bank_account_no")]
        public string? BankAccountNo { get; set; }

        [Column("bank_promptpay_id")]
        public string? BankPromptPayId { get; set; } //สำหรับบัญชีที่เป็น PromptPay PromptPayId จะเป็นหมายเลขโทรศัพท์หรือหมายเลขบัตรประชาชนที่ลงทะเบียนกับ PromptPay

        [Column("bank_account_type")]
        public string? BankAccountType { get; set; } // Native, PromptPay

        [Column("bank_config")]
        public string? BankConfig { get; set; } //JSON string to represent object เช่น config specific สำหรับ bank นั้น ๆ

        [Column("bank_is_native_qr_support")]
        public bool BankIsNativeQrSupport { get; set; }


        //ข้อมูลเกี่ยวกับ policy ต่าง ๆ เช่น limit ต่อวัน ฯลฯ
        [Column("tx_min_amount")]
        public decimal? TxMinAmount { get; set; }

        [Column("tx_max_amount")]
        public decimal? TxMaxAmount { get; set; }

        [Column("daily_total_amount_limit")]
        public decimal? DailyTotalAmountLimit { get; set; } //ยอดเงินรวมสูงสุดที่สามารถทำธุรกรรมได้ในแต่ละวัน

        [Column("daily_total_count_limit")]
        public int? DailyTotalCountLimit { get; set; } //จำนวนครั้งรวมสูงสุดที่สามารถทำธุรกรรมได้ในแต่ละวัน

        [Column("current_total_amount")]
        public decimal? CurrentTotalLimit { get; set; } //ยอดเงินที่ทำธุรกรรมในวันนั้น

        [Column("current_total_count")]
        public int? CurrentTotalCount { get; set; } //จำนวนครั้งที่ทำธุรกรรมในวันนั้น

        [Column("current_balance")]
        public decimal? CurrentBalance { get; set; } //ยอดเงินคงเหลือในบัญชี

        [Column("is_random_cent")]
        public bool? IsRandomCent { get; set; } //เอาไว้บอกว่าจะ random ทศนิยมเศษสตางค์หรือไม่

        [Column("decimal_action")]
        public string? DecimalAction { get; set; } /* Round, RoundUp, Truncate */ //เอาไว้บอกว่าหากยอด request เข้ามามีทศนิยมจะทำอย่างไร


        //System fields
        [Column("created_date")]
        public DateTime? CreatedDate { get; set; }


        //Extra fields not mapped
        [NotMapped]
        public int? MerchantLinkCount { get; set; }

        [NotMapped]
        public decimal? CurrentWalletBalance { get; set; }

        [NotMapped]
        public decimal? CurrentDailyTxAmount { get; set; }

        [NotMapped]
        public MBankAccountConfig? BankConfigObj { get; set; }


        public MCurrencyAccount()
        {
            Id = Guid.NewGuid();
            CreatedDate = DateTime.UtcNow;
            BankIsNativeQrSupport = false;
            IsRandomCent = false;
            DecimalAction = ""; //ไม่ต้องทำอะไร กรอกเข้ามาอย่างไรก็ใช้อย่างนั้น
            CryptoDecimal = 6;
        }
    }
}
