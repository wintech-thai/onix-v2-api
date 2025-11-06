using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;
using LinqKit;
using Microsoft.EntityFrameworkCore;

namespace Its.Onix.Api.Database.Repositories
{
    public class AdminUserRepository : BaseRepository, IAdminUserRepository
    {
        public AdminUserRepository(IDataContext ctx)
        {
            context = ctx;
        }

        public async Task<MAdminUser> GetUserById(string userId)
        {
            Guid id = Guid.Parse(userId);

            var result = await context!.AdminUsers!
                .Join(context!.Users!,
                    au => au.UserName,
                    u => u.UserName,
                    (au, u) => new MAdminUser
                    {
                        UserId = au.UserId,
                        UserName = au.UserName,
                        RolesList = au.RolesList,
                        CreatedDate = au.CreatedDate,
                        UserEmail = u.UserEmail
                    })
                .Where(x => x.AdminUserId!.Equals(id)).FirstOrDefaultAsync();

            return result!;
        }

        public async Task<MAdminUser> GetUserByIdLeftJoin(string userId)
        {
            Guid id = Guid.Parse(userId);

            var result = await (
                from au in context!.AdminUsers
                join u in context.Users!
                    on au.UserName equals u.UserName into userGroup
                from u in userGroup.DefaultIfEmpty()  // 👈 ตรงนี้ทำให้เป็น LEFT JOIN
                where au.AdminUserId == id
                select new MAdminUser
                {
                    UserId = au.UserId,
                    UserName = au.UserName,
                    RolesList = au.RolesList,
                    CreatedDate = au.CreatedDate,
                    UserEmail = u != null ? u.UserEmail : null,
                    UserStatus = au.UserStatus,
                    PreviousUserStatus = au.PreviousUserStatus,
                    InvitedDate = au.InvitedDate,
                    TmpUserEmail = au.TmpUserEmail,
                }
            ).FirstOrDefaultAsync();

            return result!;
        }

        public async Task<MAdminUser> AddUser(MAdminUser user)
        {
            user.AdminUserId = Guid.NewGuid();
            user.CreatedDate = DateTime.UtcNow;

            await context!.AdminUsers!.AddAsync(user);
            await context.SaveChangesAsync();

            return user;
        }

        public async Task<MAdminUser?> DeleteUserById(string userId)
        {
            Guid id = Guid.Parse(userId);

            var r = await context!.AdminUsers!.Where(x => x.AdminUserId.Equals(id)).FirstOrDefaultAsync();
            if (r != null)
            {
                context!.AdminUsers!.Remove(r);
                await context.SaveChangesAsync();
            }

            return r;
        }

        private ExpressionStarter<MAdminUser> UserPredicate(VMAdminUser param)
        {
            var pd = PredicateBuilder.New<MAdminUser>();

            if ((param.FullTextSearch != "") && (param.FullTextSearch != null))
            {
                var fullTextPd = PredicateBuilder.New<MAdminUser>();
                fullTextPd = fullTextPd.Or(p => p.UserEmail!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.UserName!.Contains(param.FullTextSearch));

                pd = pd.And(fullTextPd);
            }

            return pd;
        }

        public async Task<IEnumerable<MAdminUser>> GetUsersLeftJoin(VMAdminUser param)
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

            var arr = await (from au in context!.AdminUsers
                             join u in context.Users!
                             on au.UserName equals u.UserName into userGroup
                             from user in userGroup.DefaultIfEmpty()
                             select new MAdminUser
                             {
                                 UserId = au.UserId,
                                 UserName = au.UserName,
                                 RolesList = au.RolesList,
                                 CreatedDate = au.CreatedDate,
                                 UserEmail = user != null ? user.UserEmail : null,
                                 TmpUserEmail = au.TmpUserEmail,
                                 UserStatus = au.UserStatus,
                                 PreviousUserStatus = au.PreviousUserStatus,
                                 InvitedDate = au.InvitedDate,
                             })
                .Where(predicate)
                .OrderByDescending(e => e.CreatedDate)
                .Skip(offset)
                .Take(limit)
                .ToListAsync();

            return arr;
        }

        public async Task<int> GetUserCountLeftJoin(VMAdminUser param)
        {
            var predicate = UserPredicate(param);

            var cnt = await (
                from au in context!.AdminUsers
                join u in context.Users!
                    on au.UserName equals u.UserName into userGroup
                from user in userGroup.DefaultIfEmpty()
                select new MAdminUser
                {
                    UserId = au.UserId,
                    UserName = au.UserName,
                    RolesList = au.RolesList,
                    CreatedDate = au.CreatedDate,
                    UserEmail = user != null ? user.UserEmail : null,
                    TmpUserEmail = au.TmpUserEmail,
                    UserStatus = au.UserStatus,
                    PreviousUserStatus = au.PreviousUserStatus,
                    InvitedDate = au.InvitedDate,
                }
            )
            .Where(predicate)
            .CountAsync();

            return cnt;
        }

        public async Task<MAdminUser?> UpdateUserById(MAdminUser user)
        {
            var result = await context!.AdminUsers!.Where(x => x.AdminUserId!.Equals(user.AdminUserId)).FirstOrDefaultAsync();

            if (result != null)
            {
                result.RolesList = user.RolesList;
                result.Tags = user.Tags;

                await context!.SaveChangesAsync();
            }

            return result!;
        }

        public async Task<bool> IsUserNameExist(string userName)
        {
            var cnt = await context!.AdminUsers!.Where(p => p!.UserName!.Equals(userName)).CountAsync();
            return cnt >= 1;
        }

        public async Task<MAdminUser?> UpdateUserStatusById(string userId, string status)
        {
            Guid id = Guid.Parse(userId);
            var result = await context!.AdminUsers!.Where(x => x.AdminUserId!.Equals(id)).FirstOrDefaultAsync();

            if (result != null)
            {
                result.PreviousUserStatus = result.UserStatus;
                result.UserStatus = status;

                await context!.SaveChangesAsync();
            }

            return result!;
        }

        public async Task<MAdminUser?> UpdateUserStatusById(string adminUserId, string userId, string status)
        {
            Guid id = Guid.Parse(adminUserId);
            var result = await context!.AdminUsers!.Where(x => x.AdminUserId!.Equals(id)).FirstOrDefaultAsync();

            if (result != null)
            {
                result.UserId = userId;
                result.PreviousUserStatus = result.UserStatus;
                result.UserStatus = status;

                await context!.SaveChangesAsync();
            }

            return result!;
        }
    }
}