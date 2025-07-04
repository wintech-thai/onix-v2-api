using LinqKit;
using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Database.Repositories
{
    public class SystemVariableRepository : BaseRepository, ISystemVariableRepository
    {
        public SystemVariableRepository(IDataContext ctx)
        {
            context = ctx;
        }

        public MSystemVariable AddSystemVariable(MSystemVariable systemVariable)
        {
            systemVariable.VariableId = Guid.NewGuid();
            systemVariable.CreatedDate = DateTime.UtcNow;
            systemVariable.OrgId = orgId;

            context!.SystemVariables!.Add(systemVariable);
            context.SaveChanges();

            return systemVariable;
        }

        private ExpressionStarter<MSystemVariable> SystemVariablePredicate(VMSystemVariable param)
        {
            var pd = PredicateBuilder.New<MSystemVariable>();

            pd = pd.And(p => p.OrgId!.Equals(orgId));

            if ((param.FullTextSearch != "") && (param.FullTextSearch != null))
            {
                var fullTextPd = PredicateBuilder.New<MSystemVariable>();
                fullTextPd = fullTextPd.Or(p => p.VariableName!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.Description!.Contains(param.FullTextSearch));

                pd = pd.And(fullTextPd);
            }

            return pd;
        }

        public int GetSystemVariableCount(VMSystemVariable param)
        {
            var predicate = SystemVariablePredicate(param);
            var cnt = context!.SystemVariables!.Where(predicate).Count();

            return cnt;
        }

        public IEnumerable<MSystemVariable> GetSystemVariables(VMSystemVariable param)
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

            var predicate = SystemVariablePredicate(param!);
            var arr = context!.SystemVariables!.Where(predicate)
                .OrderByDescending(e => e.VariableName)
                .Skip(offset)
                .Take(limit)
                .ToList();

            return arr;
        }

        public MSystemVariable GetSystemVariableById(string ipMapId)
        {
            Guid id = Guid.Parse(ipMapId);

            var u = context!.SystemVariables!.Where(p => p!.VariableId!.Equals(id) && p!.OrgId!.Equals(orgId)).FirstOrDefault();
            return u!;
        }

        public MSystemVariable GetSystemVariableByName(string systemVariableName)
        {
            var u = context!.SystemVariables!.Where(p => p!.VariableName!.Equals(systemVariableName) && p!.OrgId!.Equals(orgId)).FirstOrDefault();
            return u!;
        }

        public bool IsSystemVariableNameExist(string systemVariableName)
        {
            var cnt = context!.SystemVariables!.Where(p => p!.VariableName!.Equals(systemVariableName)
                && p!.OrgId!.Equals(orgId)).Count();

            return cnt >= 1;
        }

        public MSystemVariable? DeleteSystemVariableById(string SystemVariableId)
        {
            Guid id = Guid.Parse(SystemVariableId);

            var r = context!.SystemVariables!.Where(x => x.OrgId!.Equals(orgId) && x.VariableId.Equals(id)).FirstOrDefault();
            if (r != null)
            {
                context!.SystemVariables!.Remove(r);
                context.SaveChanges();
            }

            return r;
        }

        public MSystemVariable? UpdateSystemVariableById(string systemVariableId, MSystemVariable systemVariable)
        {
            Guid id = Guid.Parse(systemVariableId);
            var result = context!.SystemVariables!.Where(x => x.OrgId!.Equals(orgId) && x.VariableId!.Equals(id)).FirstOrDefault();

            if (result != null)
            {
                //Not allow to update variable name
                result.Description = systemVariable.Description;
                result.VariableValue = systemVariable.VariableValue;

                context!.SaveChanges();
            }

            return result!;
        }
    }
}