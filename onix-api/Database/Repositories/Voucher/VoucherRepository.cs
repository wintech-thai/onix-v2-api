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
            var result = (
                from vc in context!.Vouchers

                // LEFT JOIN Items
                join prd in context.Items!
                    on vc.PrivilegeId equals prd.Id.ToString() into items
                from item in items.DefaultIfEmpty()

                // LEFT JOIN Entities (Customers)
                join cst in context.Entities!
                    on vc.CustomerId equals cst.Id.ToString() into customers
                from customer in customers.DefaultIfEmpty()

                select new MVoucher
                {
                    Id = vc.Id,
                    OrgId = vc.OrgId,
                    VoucherNo = vc.VoucherNo,
                    VoucherParams = vc.VoucherParams,
                    Description = vc.Description,
                    CustomerId = vc.CustomerId,
                    WalletId = vc.WalletId,
                    PrivilegeId = vc.PrivilegeId,
                    Tags = vc.Tags,
                    StartDate = vc.StartDate,
                    EndDate = vc.EndDate,
                    RedeemPrice = vc.RedeemPrice,
                    Status = vc.Status,
                    IsUsed = vc.IsUsed,
                    CustomerEmail = customer != null ? customer.PrimaryEmail : "",
                    CustomerName = customer != null ? customer.Name : "",
                    CustomerCode = customer != null ? customer.Code : "",
                    PrivilegeCode = item != null ? item.Code : "",
                    PrivilegeName = item != null ? item.Description : "",
                }
            );

            return result;
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
            }

            return result;
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
