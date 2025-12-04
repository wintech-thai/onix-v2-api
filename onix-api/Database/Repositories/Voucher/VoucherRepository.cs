using LinqKit;
using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;
using Microsoft.EntityFrameworkCore;

namespace Its.Onix.Api.Database.Repositories
{
    public class VoucherRepository : BaseRepository, IVoucherRepository
    {
        public VoucherRepository(IDataContext ctx)
        {
            context = ctx;
        }

        public async Task<bool> IsVoucherNoExist(string docNo)
        {
            var exists = await context!.Vouchers!.AnyAsync(p => p!.VoucherNo!.Equals(docNo) && p!.OrgId!.Equals(orgId));
            return exists;
        }

        public async Task<MVoucher> AddVoucher(MVoucher vc)
        {
            vc.OrgId = orgId;

            await context!.Vouchers!.AddAsync(vc);
            await context.SaveChangesAsync();

            return vc;
        }

        public async Task<MVoucher?> UpdateVoucherStatusById(string voucherId, string status)
        {
            Guid id = Guid.Parse(voucherId);
            var existing = await context!.Vouchers!.Where(p => p!.Id!.Equals(id) && p!.OrgId!.Equals(orgId)).FirstOrDefaultAsync();
            if (existing != null)
            {
                existing.Status = status;
            }

            await context.SaveChangesAsync();
            return existing;
        }

        public async Task<MVoucher?> UpdateVoucherIsUsedFlagById(string voucherId, string isUseFlag)
        {
            Guid id = Guid.Parse(voucherId);
            var existing = await context!.Vouchers!.Where(p => p!.Id!.Equals(id) && p!.OrgId!.Equals(orgId)).FirstOrDefaultAsync();
            if (existing != null)
            {
                existing.IsUsed = isUseFlag;
            }

            await context.SaveChangesAsync();
            return existing;
        }

        public async Task<MVoucher?> DeleteVoucherById(string voucherId)
        {
            Guid id = Guid.Parse(voucherId);
            var existing = await context!.Vouchers!.Where(p => p!.Id!.Equals(id) && p!.OrgId!.Equals(orgId)).FirstOrDefaultAsync();
            if (existing != null)
            {
                context.Vouchers!.Remove(existing);
                await context.SaveChangesAsync();
            }

            return existing;
        }

        private ExpressionStarter<MVoucher> VoucherPredicate(VMVoucher param)
        {
            var pd = PredicateBuilder.New<MVoucher>();

            pd = pd.And(p => p.OrgId!.Equals(orgId));

            if ((param.FullTextSearch != "") && (param.FullTextSearch != null))
            {
                var fullTextPd = PredicateBuilder.New<MVoucher>();
                fullTextPd = fullTextPd.Or(p => p.VoucherNo!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.Description!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.Tags!.Contains(param.FullTextSearch));

                pd = pd.And(fullTextPd);
            }

            return pd;
        }

        public IQueryable<MVoucher> GetSelection()
        {
            var query =
                from vc in context!.Vouchers

                join prd in context.Items!
                    on vc.PrivilegeId equals prd.Id.ToString() into prdGroup
                from item in prdGroup.DefaultIfEmpty()

                join cst in context.Entities!
                    on vc.CustomerId! equals cst.Id.ToString() into cstGroup
                from customer in cstGroup.DefaultIfEmpty()

                select new { vc, item, customer };  // <-- ให้ query ตรงนี้ยังเป็น IQueryable

            // จากนั้นค่อย map เป็น MVoucher (ยังเป็น IQueryable)
            return query.Select(x => new MVoucher
            {
                Id = x.vc.Id,
                OrgId = x.vc.OrgId,
                VoucherNo = x.vc.VoucherNo,
                VoucherParams = x.vc.VoucherParams,
                Description = x.vc.Description,
                CustomerId = x.vc.CustomerId,
                WalletId = x.vc.WalletId,
                PrivilegeId = x.vc.PrivilegeId,
                Tags = x.vc.Tags,
                StartDate = x.vc.StartDate,
                EndDate = x.vc.EndDate,
                RedeemPrice = x.vc.RedeemPrice,
                Status = x.vc.Status,
                IsUsed = x.vc.IsUsed,
                Barcode = x.vc.Barcode,
                Pin = x.vc.Pin,

                CustomerEmail = x.customer != null ? x.customer.PrimaryEmail : "",
                CustomerName = x.customer != null ? x.customer.Name : "",
                CustomerCode = x.customer != null ? x.customer.Code : "",

                PrivilegeCode = x.item != null ? x.item.Code : "",
                PrivilegeName = x.item != null ? x.item.Description : "",

                CreatedDate = x.vc.CreatedDate,
            });
        }

        public async Task<List<MVoucher>> GetVouchers(VMVoucher param)
        {
            var limit = 0;
            var offset = 0;

            //Param will never be null
            if (param.Offset > 0)
            {
                //Convert to zero base
                offset = param.Offset-1;
            }

            if (param.Limit > 0)
            {
                limit = param.Limit;
            }

            var predicate = VoucherPredicate(param!);
            var result = await GetSelection()
            .Where(predicate)
            .OrderByDescending(e => e.CreatedDate)
            .Skip(offset)
            .Take(limit)
            .ToListAsync();

            //มันใหญ่มากเลยไม่อยากให้ return params ออกไป
            foreach (var r in result)
            {
                r.VoucherParams = "";
                r.Pin = "******";
            }

            return result;
        }

        public async Task<MVoucher?> VerifyVoucherByBarcode(string barcode)
        {
            var u = await GetSelection().Where(p => p!.Barcode!.Equals(barcode) && p!.OrgId!.Equals(orgId)).FirstOrDefaultAsync();
            return u;
        }

        public Task<MVoucher?> VerifyVoucherByPin(string voucherNo, string pin)
        {
            var u = GetSelection().Where(p => p!.VoucherNo!.Equals(voucherNo) && p!.Pin!.Equals(pin) && p!.OrgId!.Equals(orgId)).FirstOrDefaultAsync();
            return u;
        }

        public async Task<MVoucher?> UpdateVoucherUsedFlagById(string voucherId, string isUsed)
        {
            Guid id = Guid.Parse(voucherId);
            var existing = await context!.Vouchers!.Where(p => p!.Id!.Equals(id) && p!.OrgId!.Equals(orgId)).FirstOrDefaultAsync();
            if (existing != null)
            {
                existing.IsUsed = isUsed;
                existing.UsedDate = DateTime.UtcNow;
            }

            await context.SaveChangesAsync();
            return existing;
        }

        public async Task<MVoucher?> UpdateVoucherUsedFlagById(string voucherId, string pin, string isUsed)
        {
            Guid id = Guid.Parse(voucherId);
            var existing = await context!.Vouchers!.Where(p => p!.Id!.Equals(id) && p!.Pin!.Equals(pin) && p!.OrgId!.Equals(orgId)).FirstOrDefaultAsync();
            if (existing != null)
            {
                existing.IsUsed = isUsed;
                existing.UsedDate = DateTime.UtcNow;
            }

            await context.SaveChangesAsync();
            return existing;
        }

        public async Task<int> GetVoucherCount(VMVoucher param)
        {
            var predicate = VoucherPredicate(param!);
            var result = await context!.Vouchers!.Where(predicate).CountAsync();

            return result;
        }

        public async Task<MVoucher?> GetVoucherById(string voucherId)
        {
            Guid id = Guid.Parse(voucherId);
            var u = await GetSelection().Where(p => p!.Id!.Equals(id) && p!.OrgId!.Equals(orgId)).FirstOrDefaultAsync();
            return u;
        }
    }
}
