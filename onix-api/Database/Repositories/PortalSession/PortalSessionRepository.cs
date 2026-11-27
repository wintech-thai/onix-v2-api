using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;
using Microsoft.EntityFrameworkCore;

namespace Its.Onix.Api.Database.Repositories
{
    public class PortalSessionRepository : BaseRepository, IPortalSessionRepository
    {
        public PortalSessionRepository(IDataContext ctx)
        {
            context = ctx;
        }

        public async Task<MPortalSession> AddPortalSession(MPortalSession portalSession)
        {
            portalSession.OrgId = orgId;
            portalSession.PortalSessionId = Guid.NewGuid();
            portalSession.CreatedDate = DateTime.UtcNow;

            context!.PortalSessions!.Add(portalSession);
            await context.SaveChangesAsync();

            return portalSession;
        }

        private IQueryable<MPortalSession> GetSelection(VMPortalSession param)
        {
            var query = context!.PortalSessions!.AsQueryable();

            //global ข้าม filter ให้ Admin ดูได้ทุก tenant, service layer เช็ค mismatch อีกที
            if (orgId != "global")
            {
                query = query.Where(x => x.OrgId!.Equals(orgId));
            }

            if (!string.IsNullOrEmpty(param.Status))
            {
                query = query.Where(x => x.Status!.Equals(param.Status));
            }

            if (!string.IsNullOrEmpty(param.PortalPageCode))
            {
                query = query.Where(x => x.PortalPageCode!.Equals(param.PortalPageCode));
            }

            if (!string.IsNullOrEmpty(param.FullTextSearch))
            {
                var s = param.FullTextSearch;
                query = query.Where(x =>
                    x.PortalPageCode!.Contains(s) ||
                    x.ParametersJson!.Contains(s) ||
                    (x.MacAddress != null && x.MacAddress.Contains(s)) ||
                    (x.UserName != null && x.UserName.Contains(s)));
            }

            if (param.DateFrom != null)
            {
                query = query.Where(x => x.CreatedDate >= param.DateFrom);
            }

            if (param.DateTo != null)
            {
                query = query.Where(x => x.CreatedDate <= param.DateTo);
            }

            return query;
        }

        public async Task<List<MPortalSession>> GetPortalSessions(VMPortalSession param)
        {
            var offset = param.Offset > 0 ? param.Offset - 1 : 0;
            var limit = param.Limit > 0 ? param.Limit : 0;

            var result = await GetSelection(param)
                .OrderByDescending(x => x.CreatedDate)
                .Skip(offset)
                .Take(limit)
                .ToListAsync();

            return result;
        }

        public Task<int> GetPortalSessionCount(VMPortalSession param)
        {
            return GetSelection(param).CountAsync();
        }

        public Task<MPortalSession?> GetPortalSessionById(string portalSessionId)
        {
            var id = Guid.Parse(portalSessionId);
            var query = context!.PortalSessions!.Where(x => x.PortalSessionId!.Equals(id));

            if (orgId != "global")
            {
                query = query.Where(x => x.OrgId!.Equals(orgId));
            }

            return query.FirstOrDefaultAsync();
        }

        public async Task<MPortalSession?> UpdatePortalSessionStatusById(string portalSessionId, string status, string? statusDesc, string? userName = null, string? googleClaimsJson = null)
        {
            var id = Guid.Parse(portalSessionId);
            var result = await context!.PortalSessions!.Where(x => x.OrgId!.Equals(orgId) && x.PortalSessionId!.Equals(id)).FirstOrDefaultAsync();

            if (result != null)
            {
                result.Status = status;
                result.StatusDesc = statusDesc;
                if (userName != null)
                {
                    result.UserName = userName;
                }
                if (googleClaimsJson != null)
                {
                    result.GoogleClaimsJson = googleClaimsJson;
                }
                if (status == "Authorized")
                {
                    result.LoginDate = DateTime.UtcNow;
                }
                await context!.SaveChangesAsync();
            }

            return result;
        }
    }
}
