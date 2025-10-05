using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using IndexAttribute = Microsoft.EntityFrameworkCore.IndexAttribute;
using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.Models
{
    [ExcludeFromCodeCoverage]
    [Table("Stats")]

    [Index(nameof(OrgId))]
    [Index(nameof(StatCode))]
    [Index(nameof(BalanceDate))]
    public class MStat
    {
        [Key]
        [Column("stat_id")]
        public Guid? Id { get; set; }

        [Column("org_id")]
        public string? OrgId { get; set; }

        [Column("stat_code")] //ScanItemBalanceDaily, ScanItemBalanceCurrent
        public string? StatCode { get; set; }

        [Column("balance_date")]
        public DateTime? BalanceDate { get; set; }

        [Column("balance_date_key")] //000000 for current balance
        public string? BalanceDateKey { get; set; }

        [Column("tx_in")]
        public long? TxIn { get; set; }

        [Column("tx_out")]
        public long? TxOut { get; set; }

        [Column("balance_begin")]
        public long? BalanceBegin { get; set; }

        [Column("balance_end")]
        public long? BalanceEnd { get; set; }


        //System fields
        [Column("created_date")]
        public DateTime? CreatedDate { get; set; }

        public MStat()
        {
            Id = Guid.NewGuid();
            CreatedDate = DateTime.UtcNow;
        }
    }
}
