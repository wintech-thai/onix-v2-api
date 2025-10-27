using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;
using Microsoft.EntityFrameworkCore;
using LinqKit;

namespace Its.Onix.Api.Database.Repositories
{
    public class ApiKeyRepository : BaseRepository, IApiKeyRepository
    {
        public ApiKeyRepository(IDataContext ctx)
        {
            context = ctx;
        }

        public Task<MApiKey> GetApiKey(string apiKey)
        {
            var result = context!.ApiKeys!.Where(x => x.OrgId!.Equals(orgId) && x.ApiKey!.Equals(apiKey)).FirstOrDefaultAsync();
            return result!;
        }

        public Task<MApiKey> GetApiKeyByName(string keyName)
        {
            var result = context!.ApiKeys!.Where(x => x.OrgId!.Equals(orgId) && x.KeyName!.Equals(keyName)).FirstOrDefaultAsync();
            return result!;
        }

        public MApiKey AddApiKey(MApiKey apiKey)
        {
            apiKey.KeyId = Guid.NewGuid();
            apiKey.KeyCreatedDate = DateTime.UtcNow;
            apiKey.OrgId = orgId;

            context!.ApiKeys!.Add(apiKey);
            context.SaveChanges();

            return apiKey;
        }

        public MApiKey? DeleteApiKeyById(string keyId)
        {
            Guid id = Guid.Parse(keyId);

            var r = context!.ApiKeys!.Where(x => x.OrgId!.Equals(orgId) && x.KeyId.Equals(id)).FirstOrDefault();
            if (r != null)
            {
                context!.ApiKeys!.Remove(r);
                context.SaveChanges();
            }

            return r;
        }

        private ExpressionStarter<MApiKey> ApiKeyPredicate(VMApiKey param)
        {
            var pd = PredicateBuilder.New<MApiKey>();

            pd = pd.And(p => p.OrgId!.Equals(orgId));

            if ((param.FullTextSearch != "") && (param.FullTextSearch != null))
            {
                var fullTextPd = PredicateBuilder.New<MApiKey>();
                fullTextPd = fullTextPd.Or(p => p.KeyDescription!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.KeyName!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.RolesList!.Contains(param.FullTextSearch));

                pd = pd.And(fullTextPd);
            }

            return pd;
        }

        public IEnumerable<MApiKey> GetApiKeys(VMApiKey param)
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

            var predicate = ApiKeyPredicate(param!);
            var arr = context!.ApiKeys!.Where(predicate)
                .OrderByDescending(e => e.KeyCreatedDate)
                .Skip(offset)
                .Take(limit)
                .ToList();

            return arr;
        }

        public Task<MApiKey> GetApiKeyById(string keyId)
        {
            Guid id = Guid.Parse(keyId);
            var result = context!.ApiKeys!.Where(x => x.OrgId!.Equals(orgId) && x.KeyId!.Equals(id)).FirstOrDefaultAsync();

            return result!;
        }

        public int GetApiKeyCount(VMApiKey param)
        {
            var predicate = ApiKeyPredicate(param);
            var cnt = context!.ApiKeys!.Where(predicate).Count();

            return cnt;
        }

        public MApiKey? UpdateApiKeyById(string keyId, MApiKey apiKey)
        {
            Guid id = Guid.Parse(keyId);
            var result = context!.ApiKeys!.Where(x => x.OrgId!.Equals(orgId) && x.KeyId!.Equals(id)).FirstOrDefault();

            if (result != null)
            {
                result.KeyDescription = apiKey.KeyDescription;
                result.RolesList = apiKey.RolesList;
                result.KeyExpiredDate = apiKey.KeyExpiredDate;

                context!.SaveChanges();
            }

            return result!;
        }

        public MApiKey? UpdateApiKeyStatusById(string keyId, string status)
        {
            Guid id = Guid.Parse(keyId);
            var result = context!.ApiKeys!.Where(x => x.OrgId!.Equals(orgId) && x.KeyId!.Equals(id)).FirstOrDefault();

            if (result != null)
            {
                result.KeyStatus = status;
                context!.SaveChanges();
            }

            return result!;
        }
    }
}