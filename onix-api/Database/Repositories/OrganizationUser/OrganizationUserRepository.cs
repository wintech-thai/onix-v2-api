using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;
using LinqKit;
using Microsoft.EntityFrameworkCore;

namespace Its.Onix.Api.Database.Repositories
{
    public class OrganizationUserRepository : BaseRepository, IOrganizationUserRepository
    {
        public OrganizationUserRepository(IDataContext ctx)
        {
            context = ctx;
        }

        public Task<MOrganizationUser> GetUserById(string orgUserId)
        {
            Guid id = Guid.Parse(orgUserId);

            var result = context!.OrganizationUsers!
                .Join(context!.Users!,
                    ou => ou.UserName,
                    u => u.UserName,
                    (ou, u) => new MOrganizationUser
                    {
                        OrgUserId = ou.OrgUserId,
                        OrgCustomId = ou.OrgCustomId,
                        UserId = ou.UserId,
                        UserName = ou.UserName,
                        RolesList = ou.RolesList,
                        CreatedDate = ou.CreatedDate,
                        UserEmail = u.UserEmail
                    })
                .Where(x => x.OrgCustomId!.Equals(orgId) && x.OrgUserId!.Equals(id)).FirstOrDefaultAsync();

            return result!;
        }

        public Task<MOrganizationUser> GetUserByIdLeftJoin(string orgUserId)
        {
            Guid id = Guid.Parse(orgUserId);

            var result = (
                from ou in context!.OrganizationUsers
                join u in context.Users!
                    on ou.UserName equals u.UserName into userGroup
                from u in userGroup.DefaultIfEmpty()  // 👈 ตรงนี้ทำให้เป็น LEFT JOIN
                where ou.OrgCustomId == orgId && ou.OrgUserId == id
                select new MOrganizationUser
                {
                    OrgUserId = ou.OrgUserId,
                    OrgCustomId = ou.OrgCustomId,
                    UserId = ou.UserId,
                    UserName = ou.UserName,
                    RolesList = ou.RolesList,
                    CreatedDate = ou.CreatedDate,
                    UserEmail = u != null ? u.UserEmail : null,
                    UserStatus = ou.UserStatus,
                    PreviousUserStatus = ou.PreviousUserStatus,
                    InvitedDate = ou.InvitedDate,
                    IsOrgInitialUser = ou.IsOrgInitialUser,
                    TmpUserEmail = ou.TmpUserEmail,
                }
            ).FirstOrDefaultAsync();

            return result!;
        }

        public MOrganizationUser AddUser(MOrganizationUser user)
        {
            user.OrgUserId = Guid.NewGuid();
            user.CreatedDate = DateTime.UtcNow;
            user.OrgCustomId = orgId;

            context!.OrganizationUsers!.Add(user);
            context.SaveChanges();

            return user;
        }

        public MOrganizationUser? DeleteUserById(string orgUserId)
        {
            Guid id = Guid.Parse(orgUserId);

            var r = context!.OrganizationUsers!.Where(x => x.OrgCustomId!.Equals(orgId) && x.OrgUserId.Equals(id)).FirstOrDefault();
            if (r != null)
            {
                context!.OrganizationUsers!.Remove(r);
                context.SaveChanges();
            }

            return r;
        }

        public IEnumerable<MOrganizationUser> GetUsers(VMOrganizationUser param)
        {
            var limit = 0;
            var offset = 0;

            //Param will never be null
            if (param.Offset > 0)
            {
                //Convert to zero base
                offset = param.Offset - 1;
            }

            if (param.Limit > 0)
            {
                limit = param.Limit;
            }

            var predicate = UserPredicate(param!);

            var arr = context!.OrganizationUsers!
                .Join(context!.Users!,
                    ou => ou.UserName,
                    u => u.UserName,
                    (ou, u) => new MOrganizationUser
                    {
                        OrgUserId = ou.OrgUserId,
                        OrgCustomId = ou.OrgCustomId,
                        UserId = ou.UserId,
                        UserName = ou.UserName,
                        RolesList = ou.RolesList,
                        CreatedDate = ou.CreatedDate,
                        UserEmail = u.UserEmail
                    })
                .AsQueryable()
                .AsExpandable()
                .Where(predicate)
                .OrderByDescending(e => e.CreatedDate)
                .Skip(offset)
                .Take(limit)
                .ToList();

            return arr;
        }

        public IEnumerable<MOrganizationUser> GetUsersLeftJoin(VMOrganizationUser param)
        {
            var limit = 0;
            var offset = 0;

            //Param will never be null
            if (param.Offset > 0)
            {
                //Convert to zero base
                offset = param.Offset - 1;
            }

            if (param.Limit > 0)
            {
                limit = param.Limit;
            }

            var predicate = UserPredicate(param!);

            var arr = (from ou in context!.OrganizationUsers
                       join u in context.Users!
                       on ou.UserName equals u.UserName into userGroup
                       from user in userGroup.DefaultIfEmpty()
                       select new MOrganizationUser
                       {
                           OrgUserId = ou.OrgUserId,
                           OrgCustomId = ou.OrgCustomId,
                           UserId = ou.UserId,
                           UserName = ou.UserName,
                           RolesList = ou.RolesList,
                           CreatedDate = ou.CreatedDate,
                           UserEmail = user != null ? user.UserEmail : null,
                           UserStatus = ou.UserStatus,
                           PreviousUserStatus = ou.PreviousUserStatus,
                           InvitedDate = ou.InvitedDate,
                           IsOrgInitialUser = ou.IsOrgInitialUser,
                       })
                .Where(predicate)
                .OrderByDescending(e => e.CreatedDate)
                .Skip(offset)
                .Take(limit)
                .ToList();

            return arr;
        }

        private ExpressionStarter<MOrganizationUser> UserPredicate(VMOrganizationUser param)
        {
            var pd = PredicateBuilder.New<MOrganizationUser>();

            pd = pd.And(p => p.OrgCustomId!.Equals(orgId));

            if ((param.FullTextSearch != "") && (param.FullTextSearch != null))
            {
                var fullTextPd = PredicateBuilder.New<MOrganizationUser>();
                fullTextPd = fullTextPd.Or(p => p.UserEmail!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.UserName!.Contains(param.FullTextSearch));

                pd = pd.And(fullTextPd);
            }

            return pd;
        }

        public int GetUserCount(VMOrganizationUser param)
        {
            var predicate = UserPredicate(param);
            var cnt = context!.OrganizationUsers!.Where(predicate).Count();

            return cnt;
        }

        public int GetUserCountLeftJoin(VMOrganizationUser param)
        {
            var predicate = UserPredicate(param);

            var cnt = (
                from ou in context!.OrganizationUsers
                join u in context.Users!
                    on ou.UserName equals u.UserName into userGroup
                from user in userGroup.DefaultIfEmpty()
                select new MOrganizationUser
                {
                    OrgUserId = ou.OrgUserId,
                    OrgCustomId = ou.OrgCustomId,
                }
            )
            .Where(predicate)
            .Count();

            return cnt;
        }

        public MOrganizationUser? UpdateUserById(string orgUserId, MOrganizationUser user)
        {
            Guid id = Guid.Parse(orgUserId);
            var result = context!.OrganizationUsers!.Where(x => x.OrgCustomId!.Equals(orgId) && x.OrgUserId!.Equals(id)).FirstOrDefault();

            if (result != null)
            {
                result.RolesList = user.RolesList;

                context!.SaveChanges();
            }

            return result!;
        }

        public MOrganizationUser? UpdateUserStatusById(string orgUserId, string userId, string status)
        {
            Guid id = Guid.Parse(orgUserId);
            var result = context!.OrganizationUsers!.Where(x => x.OrgCustomId!.Equals(orgId) && x.OrgUserId!.Equals(id)).FirstOrDefault();

            if (result != null)
            {
                result.PreviousUserStatus = result.UserStatus;
                result.UserStatus = status;
                result.UserId = userId;

                context!.SaveChanges();
            }

            return result!;
        }

        public bool IsUserNameExist(string userName)
        {
            var cnt = context!.OrganizationUsers!.Where(p => p.OrgCustomId!.Equals(orgId) && p!.UserName!.Equals(userName)).Count();
            return cnt >= 1;
        }
    }
}