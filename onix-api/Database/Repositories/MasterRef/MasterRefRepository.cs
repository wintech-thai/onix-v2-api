using LinqKit;
using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Database.Repositories
{
    public class MasterRefRepository : BaseRepository, IMasterRefRepository
    {
        public MasterRefRepository(IDataContext ctx)
        {
            context = ctx;
        }

        public MMasterRef AddMasterRef(MMasterRef masterRef)
        {
            masterRef.Id = Guid.NewGuid();
            masterRef.CreatedDate = DateTime.UtcNow;
            masterRef.UpdatedDate = DateTime.UtcNow;
            masterRef.OrgId = orgId;

            context!.MasterRefs!.Add(masterRef);
            context.SaveChanges();

            return masterRef;
        }

        private ExpressionStarter<MMasterRef> MasterRefPredicate(VMMasterRef param)
        {
            var pd = PredicateBuilder.New<MMasterRef>();

            pd = pd.And(p => p.OrgId!.Equals(orgId));

            if ((param.FullTextSearch != "") && (param.FullTextSearch != null))
            {
                var fullTextPd = PredicateBuilder.New<MMasterRef>();
                fullTextPd = fullTextPd.Or(p => p.Code!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.Description!.Contains(param.FullTextSearch));

                pd = pd.And(fullTextPd);
            }

            if ((param.RefType != null) && (param.RefType > 0))
            {
                var refTypePd = PredicateBuilder.New<MMasterRef>();
                refTypePd = refTypePd.Or(p => p.RefType!.Equals(param.RefType));

                pd = pd.And(refTypePd);
            }

            return pd;
        }

        public int GetMasterRefCount(VMMasterRef param)
        {
            var predicate = MasterRefPredicate(param);
            var cnt = context!.MasterRefs!.Where(predicate).Count();

            return cnt;
        }

        public IEnumerable<MMasterRef> GetMasterRefs(VMMasterRef param)
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

            var predicate = MasterRefPredicate(param!);
            var arr = context!.MasterRefs!.Where(predicate)
                .OrderByDescending(e => e.Code)
                .Skip(offset)
                .Take(limit)
                .ToList();

            return arr;
        }

        public MMasterRef GetMasterRefById(string masterRefId)
        {
            Guid id = Guid.Parse(masterRefId);

            var u = context!.MasterRefs!.Where(p => p!.Id!.Equals(id) && p!.OrgId!.Equals(orgId)).FirstOrDefault();
            return u!;
        }

        public MMasterRef GetMasterRefByName(string code)
        {
            var u = context!.MasterRefs!.Where(p => p!.Code!.Equals(code) && p!.OrgId!.Equals(orgId)).FirstOrDefault();
            return u!;
        }

        public bool IsMasterRefCodeExist(string code)
        {
            var cnt = context!.MasterRefs!.Where(p => p!.Code!.Equals(code)
                && p!.OrgId!.Equals(orgId)).Count();

            return cnt >= 1;
        }

        public MMasterRef? DeleteMasterRefById(string MasterRefId)
        {
            Guid id = Guid.Parse(MasterRefId);

            var r = context!.MasterRefs!.Where(x => x.OrgId!.Equals(orgId) && x.Id.Equals(id)).FirstOrDefault();
            if (r != null)
            {
                context!.MasterRefs!.Remove(r);
                context.SaveChanges();
            }

            return r;
        }

        public MMasterRef? UpdateMasterRefById(string masterRefId, MMasterRef masterRef)
        {
            Guid id = Guid.Parse(masterRefId);
            var result = context!.MasterRefs!.Where(x => x.OrgId!.Equals(orgId) && x.Id!.Equals(id)).FirstOrDefault();

            if (result != null)
            {
                //Not allow to update code, refType
                result.Description = masterRef.Description;
                result.UpdatedDate = DateTime.UtcNow;
                context!.SaveChanges();
            }

            return result!;
        }
    }
}