using LinqKit;
using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Database.Repositories
{
    public class EntityRepository : BaseRepository, IEntityRepository
    {
        public EntityRepository(IDataContext ctx)
        {
            context = ctx;
        }

        public MEntity AddEntity(MEntity item)
        {
            item.Id = Guid.NewGuid();
            item.CreatedDate = DateTime.UtcNow;
            item.UpdatedDate = DateTime.UtcNow;
            item.OrgId = orgId;

            context!.Entities!.Add(item);
            context.SaveChanges();

            return item;
        }

        private ExpressionStarter<MEntity> EntityPredicate(VMEntity param)
        {
            var pd = PredicateBuilder.New<MEntity>();

            pd = pd.And(p => p.OrgId!.Equals(orgId));

            if ((param.FullTextSearch != "") && (param.FullTextSearch != null))
            {
                var fullTextPd = PredicateBuilder.New<MEntity>();
                fullTextPd = fullTextPd.Or(p => p.Code!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.Name!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.Tags!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.TaxId!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.PrimaryEmail!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.SecondaryEmail!.Contains(param.FullTextSearch));

                pd = pd.And(fullTextPd);
            }

            if ((param.EntityType != null) && (param.EntityType > 0))
            {
                var typePd = PredicateBuilder.New<MEntity>();
                typePd = typePd.Or(p => p.EntityType!.Equals(param.EntityType));

                pd = pd.And(typePd);
            }

            if ((param.EntityCategory != null) && (param.EntityCategory > 0))
            {
                var categoryPd = PredicateBuilder.New<MEntity>();
                categoryPd = categoryPd.Or(p => p.EntityCategory!.Equals(param.EntityCategory));

                pd = pd.And(categoryPd);
            }

            return pd;
        }

        public int GetEntityCount(VMEntity param)
        {
            var predicate = EntityPredicate(param);
            var cnt = context!.Entities!.Where(predicate).Count();

            return cnt;
        }

        public IEnumerable<MEntity> GetEntities(VMEntity param)
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

            var predicate = EntityPredicate(param!);
            var arr = context!.Entities!.Where(predicate)
                .OrderByDescending(e => e.Code)
                .Skip(offset)
                .Take(limit)
                .ToList();

            return arr;
        }

        public MEntity GetEntityById(string itemId)
        {
            Guid id = Guid.Parse(itemId);

            var u = context!.Entities!.Where(p => p!.Id!.Equals(id) && p!.OrgId!.Equals(orgId)).FirstOrDefault();
            return u!;
        }

        public MEntity GetEntityByName(string code)
        {
            var u = context!.Entities!.Where(p => p!.Code!.Equals(code) && p!.OrgId!.Equals(orgId)).FirstOrDefault();
            return u!;
        }

        public bool IsEntityCodeExist(string code)
        {
            var cnt = context!.Entities!.Where(p => p!.Code!.Equals(code)
                && p!.OrgId!.Equals(orgId)).Count();

            return cnt >= 1;
        }

        public MEntity? DeleteEntityById(string EntityId)
        {
            Guid id = Guid.Parse(EntityId);

            var r = context!.Entities!.Where(x => x.OrgId!.Equals(orgId) && x.Id.Equals(id)).FirstOrDefault();
            if (r != null)
            {
                context!.Entities!.Remove(r);
                context.SaveChanges();
            }

            return r;
        }

        public MEntity? UpdateEntityById(string itemId, MEntity item)
        {
            Guid id = Guid.Parse(itemId);
            var result = context!.Entities!.Where(x => x.OrgId!.Equals(orgId) && x.Id!.Equals(id)).FirstOrDefault();

            if (result != null)
            {
                result.Name = item.Name;
                result.Tags = item.Tags;
                result.EntityType = item.EntityType;
                result.EntityCategory = item.EntityCategory;
                result.CreditTermDay = item.CreditTermDay;
                result.CreditAmount = item.CreditAmount;
                result.TaxId = item.TaxId;
                result.NationalCardId = item.NationalCardId;
                result.PrimaryEmail = item.PrimaryEmail;
                result.SecondaryEmail = item.SecondaryEmail;
                
                result.UpdatedDate = DateTime.UtcNow;
                context!.SaveChanges();
            }

            return result!;
        }
    }
}