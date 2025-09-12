using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.Database.Repositories;

namespace Its.Onix.Api.Services
{
    public class ScanItemService : BaseService, IScanItemService
    {
        private readonly IScanItemRepository? repository = null;

        public ScanItemService(IScanItemRepository repo) : base()
        {
            repository = repo;
        }

        public MVScanItem AttachScanItemToProduct(string orgId, string itemId, string productId)
        {
            var r = new MVScanItem()
            {
                Status = "SUCCESS",
                Description = "Success",
            };

            repository!.SetCustomOrgId(orgId);
            var result = repository!.AttachScanItemToProduct(itemId, productId);

            r.ScanItem = result;
            
            return r;
        }

        public MVScanItemResult VerifyScanItem(string orgId, string serial, string pin)
        {
            var r = new MVScanItemResult()
            {
                Status = "SUCCESS",
                DescriptionEng = $"Your product serial=[{serial}] และ pin=[{pin}] is genuine.",
                DescriptionThai = $"สินค้า ซีเรียล=[{serial}] และ พิน=[{pin}] เป็นของแท้",
            };

            repository!.SetCustomOrgId(orgId);
            var result = repository!.GetScanItemBySerialPin(serial, pin);

            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.DescriptionEng = $"No serial=[{serial}] and pin=[{pin}] in our database!!!";
                r.DescriptionThai = $"ไม่พบ ซีเรียล=[{serial}] และ พิน=[{pin}] ในฐานข้อมูล!!!";

                return r;
            }

            r.ScanItem = result;
            if (result.RegisteredFlag!.Equals("TRUE"))
            {
                r.Status = "ALREADY_REGISTERED";
                r.DescriptionEng = $"Your product serial=[{serial}] and pin=[{pin}] is already registered!!!";
                r.DescriptionThai = $"สินค้า ซีเรียล=[{serial}] และ พิน=[{pin}] เคยลงทะเบียนแล้ว!!!";

                return r;
            }

            var id = result.Id.ToString();
            r.ScanItem = repository.RegisterScanItem(id!);

            return r;
        }
    }
}
