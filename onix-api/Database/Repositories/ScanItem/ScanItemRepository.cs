using LinqKit;
using Its.Onix.Api.Models;

namespace Its.Onix.Api.Database.Repositories
{
    public class ScanItemRepository : BaseRepository, IScanItemRepository
    {
        public ScanItemRepository(IDataContext ctx)
        {
            context = ctx;
        }

        public MScanItem? GetScanItemBySerialPin(string serial, string pin)
        {
            var u = context!.ScanItems!.Where(p =>
                p!.Serial!.Equals(serial) &&
                p!.Pin!.Equals(pin) &&
                p!.OrgId!.Equals(orgId)).FirstOrDefault();

            return u!;
        }

        public MScanItem RegisterScanItem(string itemId)
        {
            Guid id = Guid.Parse(itemId);
            var result = context!.ScanItems!.Where(x => x.OrgId!.Equals(orgId) && x.Id!.Equals(id)).FirstOrDefault();

            if (result != null)
            {
                result.RegisteredFlag = "TRUE";
                result.RegisteredDate = DateTime.UtcNow;
                context!.SaveChanges();
            }

            return result!;
        }
    }
}