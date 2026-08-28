using LinqKit;
using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;
using System.Data.Entity;

namespace Its.Onix.Api.Database.Repositories
{
    public class IocRepository : BaseRepository, IIocRepository
    {
        public IocRepository(IDataContext ctx)
        {
            context = ctx;
        }

        private ExpressionStarter<MIoc> IocPredicate(VMIoc param)
        {
            var pd = PredicateBuilder.New<MIoc>();

            pd = pd.And(p => p.OrgId!.Equals(orgId));

            if ((param.FullTextSearch != "") && (param.FullTextSearch != null))
            {
                var fullTextPd = PredicateBuilder.New<MIoc>();
                fullTextPd = fullTextPd.Or(p => p.IocValue!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.Tags!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.Source!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.Noted!.Contains(param.FullTextSearch));

                pd = pd.And(fullTextPd);
            }

            if (!string.IsNullOrEmpty(param.IocType))
            {
                pd = pd.And(p => p.IocType!.Equals(param.IocType));
            }

            if (!string.IsNullOrEmpty(param.Reputation))
            {
                pd = pd.And(p => p.Reputation!.Equals(param.Reputation));
            }

            if (!string.IsNullOrEmpty(param.Status))
            {
                pd = pd.And(p => p.Status!.Equals(param.Status));
            }

            if (param.FromDate.HasValue)
            {
                pd = pd.And(p => p.CreatedDate >= param.FromDate);
            }

            if (param.ToDate.HasValue)
            {
                pd = pd.And(p => p.CreatedDate <= param.ToDate);
            }

            return pd;
        }

        public async Task<MIoc> AddIocV2(MIoc ioc)
        {
            ioc.Id = Guid.NewGuid();
            ioc.OrgId = orgId;
            ioc.CreatedDate = DateTime.UtcNow;
            ioc.FirstSeenDate = ioc.CreatedDate;
            ioc.LastSeenDate = ioc.CreatedDate;

            await context!.Iocs!.AddAsync(ioc);
            await context.SaveChangesAsync();

            return ioc;
        }

        public async Task<int> GetIocCountV2(VMIoc param)
        {
            var predicate = IocPredicate(param);
            var cnt = await context!.Iocs!.Where(predicate).AsExpandable().CountAsync();

            return cnt;
        }

        public async Task<List<MIoc>> GetIocsV2(VMIoc param)
        {
            var limit = 0;
            var offset = 0;

            if (param.Offset > 0)
            {
                offset = param.Offset - 1;
            }

            if (param.Limit > 0)
            {
                limit = param.Limit;
            }

            var predicate = IocPredicate(param);
            var arr = await context!.Iocs!.AsExpandable().Where(predicate)
                .OrderByDescending(e => e.CreatedDate)
                .Skip(offset)
                .Take(limit)
                .ToListAsync();

            return arr;
        }

        public async Task<MIoc?> GetIocByIdV2(string iocId)
        {
            Guid id = Guid.Parse(iocId);

            var u = await context!.Iocs!.AsExpandable().Where(p => p.Id!.Equals(id) && p.OrgId!.Equals(orgId)).FirstOrDefaultAsync();
            return u;
        }

        public async Task<MIoc?> DeleteIocByIdV2(string iocId)
        {
            Guid id = Guid.Parse(iocId);

            var r = await context!.Iocs!.AsExpandable().Where(p => p.OrgId!.Equals(orgId) && p.Id!.Equals(id)).FirstOrDefaultAsync();
            if (r != null)
            {
                context!.Iocs!.Remove(r);
                await context.SaveChangesAsync();
            }

            return r;
        }

        public async Task<MIoc?> UpdateIocByIdV2(string iocId, MIoc ioc)
        {
            Guid id = Guid.Parse(iocId);
            var result = await context!.Iocs!.AsExpandable().Where(p => p.OrgId!.Equals(orgId) && p.Id!.Equals(id)).FirstOrDefaultAsync();

            if (result != null)
            {
                //IocType, IocValue ล็อกไว้ ไม่ให้แก้หลังสร้างแล้ว
                result.Source = ioc.Source;
                result.RiskScore = ioc.RiskScore;
                result.ConfidenceScore = ioc.ConfidenceScore;
                result.Reputation = ioc.Reputation;
                result.Noted = ioc.Noted;
                result.Tags = ioc.Tags;
                await context.SaveChangesAsync();
            }

            return result;
        }

        public async Task<MIoc?> UpdateIocStatusByIdV2(string iocId, string status)
        {
            Guid id = Guid.Parse(iocId);
            var result = await context!.Iocs!.AsExpandable().Where(p => p.OrgId!.Equals(orgId) && p.Id!.Equals(id)).FirstOrDefaultAsync();

            if (result != null)
            {
                result.Status = status;
                await context.SaveChangesAsync();
            }

            return result;
        }
    }
}
