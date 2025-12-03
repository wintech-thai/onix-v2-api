namespace Its.Onix.Api.Models
{
    public class VoucherParam
    {
        public string WalletId { get; set; } = string.Empty;
        public string CustomerId { get; set; } = string.Empty;
        public string PrivilegeId { get; set; } = string.Empty;

        public MItemTx? ItemTransaction { get; set; }
        public MPointTx? PointTransaction { get; set; }

        public VoucherParam()
        {
        }
    }
}
