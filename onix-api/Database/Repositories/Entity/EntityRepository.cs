using LinqKit;
using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;
using Microsoft.EntityFrameworkCore;

namespace Its.Onix.Api.Database.Repositories
{
    public class EntityRepository : BaseRepository, IEntityRepository
    {
        public EntityRepository(IDataContext ctx)
        {
            context = ctx;
        }

        public MEntity GetOrCreateEntityByEmail(MEntity entity)
        {
            var email = entity.PrimaryEmail;
            var e = GetEntityByEmail(email!);
            if (e == null)
            {
                e = AddEntity(entity);
            }

            return e;
        }

        public MEntity AddEntity(MEntity item)
        {
            //ให้สร้าง wallet ให้เลยตรงนี้ เพราะว่าจะได้อยู่ใน transaction เดียวกัน

            item.Id = Guid.NewGuid();
            item.CreatedDate = DateTime.UtcNow;
            item.UpdatedDate = DateTime.UtcNow;
            item.OrgId = orgId;

            var custId = item.Id.ToString();

            var wallet = new MWallet()
            {
                OrgId = orgId,
                Id = Guid.NewGuid(),
                Name = custId,
                Description = $"Auto created wallet of customer ID [{custId}]",
                Tags = $"email={item.PrimaryEmail}",
                CustomerId = custId,
            };

            context!.Entities!.Add(item);
            context!.Wallets!.Add(wallet);

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
                fullTextPd = fullTextPd.Or(p => p.PrimaryPhone!.Contains(param.FullTextSearch));

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

            if (!string.IsNullOrEmpty(param.CustomerTypeId))
            {
                pd = pd.And(p => p.CustomerTypeId!.Equals(param.CustomerTypeId));
            }

            if (!string.IsNullOrEmpty(param.CustomerGroupId))
            {
                pd = pd.And(p => p.CustomerGroupId!.Equals(param.CustomerGroupId));
            }

            return pd;
        }

        // Entity list/detail reads only ever carry the ref id (see MEntity.CustomerTypeId/CustomerGroupId
        // comment) — join against MasterRefs here so the UI can show the code without a second round trip.
        private void PopulateCustomerRefCodes(IEnumerable<MEntity> entities)
        {
            var list = entities.ToList();
            var refIds = list
                .SelectMany(e => new[] { e.CustomerTypeId, e.CustomerGroupId })
                .Where(id => !string.IsNullOrEmpty(id))
                .Select(id => Guid.Parse(id!))
                .Distinct()
                .ToList();

            if (refIds.Count == 0) return;

            var refs = context!.MasterRefs!
                .Where(r => r.OrgId!.Equals(orgId) && refIds.Contains(r.Id!.Value))
                .ToDictionary(r => r.Id!.Value, r => r.Code);

            foreach (var e in list)
            {
                if (!string.IsNullOrEmpty(e.CustomerTypeId) && refs.TryGetValue(Guid.Parse(e.CustomerTypeId), out var typeCode))
                    e.CustomerTypeCode = typeCode;
                if (!string.IsNullOrEmpty(e.CustomerGroupId) && refs.TryGetValue(Guid.Parse(e.CustomerGroupId), out var groupCode))
                    e.CustomerGroupCode = groupCode;
            }
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
                .OrderByDescending(e => e.CreatedDate)
                .Skip(offset)
                .Take(limit)
                .ToList();

            PopulateCustomerRefCodes(arr);

            return arr;
        }

        public MEntity GetEntityByEmail(string email)
        {
            var u = context!.Entities!.Where(p => p!.PrimaryEmail!.Equals(email) && p!.OrgId!.Equals(orgId)).FirstOrDefault();
            return u!;
        }

        public MEntity GetEntityById(string itemId)
        {
            Guid id = Guid.Parse(itemId);

            var u = context!.Entities!.Where(p => p!.Id!.Equals(id) && p!.OrgId!.Equals(orgId)).FirstOrDefault();
            if (u != null)
            {
                PopulateCustomerRefCodes([u]);
                u.Addresses = context!.EntityAddresses!
                    .Where(a => a.OrgId!.Equals(orgId) && a.EntityId!.Equals(itemId))
                    .OrderByDescending(a => a.CreatedDate)
                    .ToList();
            }

            return u!;
        }

        public MEntity GetEntityByName(string code)
        {
            var u = context!.Entities!.Where(p => p!.Code!.Equals(code) && p!.OrgId!.Equals(orgId)).FirstOrDefault();
            return u!;
        }

        public bool IsEntityCodeExist(string code)
        {
            var cnt = context!.Entities!.Where(p => p!.Code!.Equals(code) && p!.OrgId!.Equals(orgId)).Count();
            return cnt >= 1;
        }

        public bool IsEntityCodeExistExcludingId(string code, string entityId)
        {
            var id = Guid.Parse(entityId);
            var cnt = context!.Entities!.Where(p => p!.Code!.Equals(code) && p!.OrgId!.Equals(orgId) && !p!.Id!.Equals(id)).Count();
            return cnt >= 1;
        }

        public bool IsPrimaryEmailExist(string email)
        {
            var cnt = context!.Entities!.Where(p => p!.PrimaryEmail!.Equals(email) && p!.OrgId!.Equals(orgId)).Count();
            return cnt >= 1;
        }

        public MEntity? DeleteEntityById(string EntityId)
        {
            Guid id = Guid.Parse(EntityId);

            var r = context!.Entities!.Where(x => x.OrgId!.Equals(orgId) && x.Id.Equals(id)).FirstOrDefault();
            if (r != null)
            {
                //ลบ table ลูก (ที่อยู่) ไปด้วย ตาม spec ข้อ 1c — ไม่มี FK cascade ระดับ DB จึงต้องลบเองตรงนี้
                var addresses = context!.EntityAddresses!.Where(a => a.OrgId!.Equals(orgId) && a.EntityId!.Equals(EntityId));
                context!.EntityAddresses!.RemoveRange(addresses);

                context!.Entities!.Remove(r);
                context.SaveChanges();
            }

            return r;
        }

        public List<MEntityAddress> GetEntityAddressesByEntityId(string entityId)
        {
            return context!.EntityAddresses!
                .Where(a => a.OrgId!.Equals(orgId) && a.EntityId!.Equals(entityId))
                .OrderByDescending(a => a.CreatedDate)
                .ToList();
        }

        public MEntityAddress? GetEntityAddressById(string addressId)
        {
            Guid id = Guid.Parse(addressId);
            return context!.EntityAddresses!.Where(a => a.OrgId!.Equals(orgId) && a.Id!.Equals(id)).FirstOrDefault();
        }

        public MEntityAddress AddEntityAddress(MEntityAddress address)
        {
            address.Id = Guid.NewGuid();
            address.CreatedDate = DateTime.UtcNow;
            address.UpdatedDate = DateTime.UtcNow;
            address.OrgId = orgId;

            context!.EntityAddresses!.Add(address);
            context.SaveChanges();

            return address;
        }

        public MEntityAddress? UpdateEntityAddressById(string addressId, MEntityAddress address)
        {
            Guid id = Guid.Parse(addressId);
            var result = context!.EntityAddresses!.Where(a => a.OrgId!.Equals(orgId) && a.Id!.Equals(id)).FirstOrDefault();

            if (result != null)
            {
                result.AddressType = address.AddressType;
                result.Address = address.Address;
                result.UpdatedDate = DateTime.UtcNow;
                context!.SaveChanges();
            }

            return result;
        }

        public MEntityAddress? DeleteEntityAddressById(string addressId)
        {
            Guid id = Guid.Parse(addressId);
            var result = context!.EntityAddresses!.Where(a => a.OrgId!.Equals(orgId) && a.Id!.Equals(id)).FirstOrDefault();

            if (result != null)
            {
                context!.EntityAddresses!.Remove(result);
                context.SaveChanges();
            }

            return result;
        }

        public MEntity? UpdateEntityEmailById(string entityId, string email)
        {
            Guid id = Guid.Parse(entityId);
            var result = context!.Entities!.Where(x => x.OrgId!.Equals(orgId) && x.Id!.Equals(id)).FirstOrDefault();

            if (result != null)
            {
                result.PrimaryEmail = email;
                result.PrimaryEmailStatus = "UNVERIFIED"; //Set status to unverified when email is changed
                result.UpdatedDate = DateTime.UtcNow;
                context!.SaveChanges();
            }

            return result;
        }

        public MEntity? UpdateEntityEmailStatusById(string entityId, string status)
        {
            Guid id = Guid.Parse(entityId);
            var result = context!.Entities!.Where(x => x.OrgId!.Equals(orgId) && x.Id!.Equals(id)).FirstOrDefault();

            if (result != null)
            {
                result.PrimaryEmailStatus = status;
                result.UpdatedDate = DateTime.UtcNow;
                context!.SaveChanges();
            }

            return result;
        }

        public MEntity? UpdateEntityUserNameById(string entityId, string userName)
        {
            Guid id = Guid.Parse(entityId);
            var result = context!.Entities!.Where(x => x.OrgId!.Equals(orgId) && x.Id!.Equals(id)).FirstOrDefault();

            if (result != null)
            {
                result.UserName = userName;
                result.UpdatedDate = DateTime.UtcNow;
                context!.SaveChanges();
            }

            return result;
        }

        public MEntity? UpdateEntityUserStatusById(string entityId, string status)
        {
            Guid id = Guid.Parse(entityId);
            var result = context!.Entities!.Where(x => x.OrgId!.Equals(orgId) && x.Id!.Equals(id)).FirstOrDefault();

            if (result != null)
            {
                result.UserStatus = status;
                result.UpdatedDate = DateTime.UtcNow;
                context!.SaveChanges();
            }

            return result;
        }

        public MEntity? UpdateUserStatusById(string entityId, string status)
        {
            Guid id = Guid.Parse(entityId);
            var result = context!.Entities!.Where(x => x.OrgId!.Equals(orgId) && x.Id!.Equals(id)).FirstOrDefault();

            if (result != null)
            {
                result.UserStatus = status;
                context!.SaveChanges();
            }

            return result!;
        }

        public MEntity? UpdateEntityById(string itemId, MEntity item)
        {
            Guid id = Guid.Parse(itemId);
            var result = context!.Entities!.Where(x => x.OrgId!.Equals(orgId) && x.Id!.Equals(id)).FirstOrDefault();

            if (result != null)
            {
                result.Code = item.Code;
                result.Name = item.Name;
                result.Tags = item.Tags;
                result.EntityType = item.EntityType;
                result.EntityCategory = item.EntityCategory;
                result.CustomerTypeId = item.CustomerTypeId;
                result.CustomerGroupId = item.CustomerGroupId;
                result.CreditTermDay = item.CreditTermDay;
                result.CreditAmount = item.CreditAmount;
                result.TaxId = item.TaxId;
                result.NationalCardId = item.NationalCardId;
                result.Content = item.Content;

                //จะไม่อนุญาตให้แก้ไข email และ phone ผ่านทาง API ตัวนี้, จะต้องใช้ API อีกตัวแทน เพราะต้องมีการตรวจสอบความถูกต้อง
                //result.SecondaryEmail = item.SecondaryEmail;
                //result.PrimaryEmail = item.PrimaryEmail;

                result.UpdatedDate = DateTime.UtcNow;
                context!.SaveChanges();
            }

            return result!;
        }

        // ──────────────────────────── V2 — async ────────────────────────────

        private async Task PopulateCustomerRefCodesV2(IEnumerable<MEntity> entities)
        {
            var list = entities.ToList();
            var refIds = list
                .SelectMany(e => new[] { e.CustomerTypeId, e.CustomerGroupId })
                .Where(id => !string.IsNullOrEmpty(id))
                .Select(id => Guid.Parse(id!))
                .Distinct()
                .ToList();

            if (refIds.Count == 0) return;

            var refs = await context!.MasterRefs!
                .Where(r => r.OrgId!.Equals(orgId) && refIds.Contains(r.Id!.Value))
                .ToDictionaryAsync(r => r.Id!.Value, r => r.Code);

            foreach (var e in list)
            {
                if (!string.IsNullOrEmpty(e.CustomerTypeId) && refs.TryGetValue(Guid.Parse(e.CustomerTypeId), out var typeCode))
                    e.CustomerTypeCode = typeCode;
                if (!string.IsNullOrEmpty(e.CustomerGroupId) && refs.TryGetValue(Guid.Parse(e.CustomerGroupId), out var groupCode))
                    e.CustomerGroupCode = groupCode;
            }
        }

        public async Task<MEntity> AddEntityV2(MEntity item)
        {
            item.Id = Guid.NewGuid();
            item.CreatedDate = DateTime.UtcNow;
            item.UpdatedDate = DateTime.UtcNow;
            item.OrgId = orgId;

            var custId = item.Id.ToString();

            var wallet = new MWallet()
            {
                OrgId = orgId,
                Id = Guid.NewGuid(),
                Name = custId,
                Description = $"Auto created wallet of customer ID [{custId}]",
                Tags = $"email={item.PrimaryEmail}",
                CustomerId = custId,
            };

            await context!.Entities!.AddAsync(item);
            await context!.Wallets!.AddAsync(wallet);

            await context.SaveChangesAsync();

            return item;
        }

        public async Task<int> GetEntityCountV2(VMEntity param)
        {
            var predicate = EntityPredicate(param);
            var cnt = await context!.Entities!.Where(predicate).AsExpandable().CountAsync();

            return cnt;
        }

        public async Task<List<MEntity>> GetEntitiesV2(VMEntity param)
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

            var predicate = EntityPredicate(param!);
            var arr = await context!.Entities!.Where(predicate).AsExpandable()
                .OrderByDescending(e => e.CreatedDate)
                .Skip(offset)
                .Take(limit)
                .ToListAsync();

            await PopulateCustomerRefCodesV2(arr);

            return arr;
        }

        public async Task<MEntity?> GetEntityByIdV2(string itemId)
        {
            Guid id = Guid.Parse(itemId);

            var u = await context!.Entities!.Where(p => p!.Id!.Equals(id) && p!.OrgId!.Equals(orgId)).FirstOrDefaultAsync();
            if (u != null)
            {
                await PopulateCustomerRefCodesV2([u]);
                u.Addresses = await context!.EntityAddresses!
                    .Where(a => a.OrgId!.Equals(orgId) && a.EntityId!.Equals(itemId))
                    .OrderByDescending(a => a.CreatedDate)
                    .ToListAsync();
            }

            return u;
        }

        public async Task<bool> IsEntityCodeExistV2(string code)
        {
            var cnt = await context!.Entities!.Where(p => p!.Code!.Equals(code) && p!.OrgId!.Equals(orgId)).CountAsync();
            return cnt >= 1;
        }

        public async Task<bool> IsEntityCodeExistExcludingIdV2(string code, string entityId)
        {
            var id = Guid.Parse(entityId);
            var cnt = await context!.Entities!.Where(p => p!.Code!.Equals(code) && p!.OrgId!.Equals(orgId) && !p!.Id!.Equals(id)).CountAsync();
            return cnt >= 1;
        }

        public async Task<bool> IsPrimaryEmailExistV2(string email)
        {
            var cnt = await context!.Entities!.Where(p => p!.PrimaryEmail!.Equals(email) && p!.OrgId!.Equals(orgId)).CountAsync();
            return cnt >= 1;
        }

        public async Task<MEntity?> DeleteEntityByIdV2(string entityId)
        {
            Guid id = Guid.Parse(entityId);

            var r = await context!.Entities!.Where(x => x.OrgId!.Equals(orgId) && x.Id.Equals(id)).FirstOrDefaultAsync();
            if (r != null)
            {
                var addresses = context!.EntityAddresses!.Where(a => a.OrgId!.Equals(orgId) && a.EntityId!.Equals(entityId));
                context!.EntityAddresses!.RemoveRange(addresses);

                context!.Entities!.Remove(r);
                await context.SaveChangesAsync();
            }

            return r;
        }

        public async Task<MEntity?> UpdateEntityByIdV2(string itemId, MEntity item)
        {
            Guid id = Guid.Parse(itemId);
            var result = await context!.Entities!.Where(x => x.OrgId!.Equals(orgId) && x.Id!.Equals(id)).FirstOrDefaultAsync();

            if (result != null)
            {
                result.Code = item.Code;
                result.Name = item.Name;
                result.Tags = item.Tags;
                result.EntityType = item.EntityType;
                result.EntityCategory = item.EntityCategory;
                result.CustomerTypeId = item.CustomerTypeId;
                result.CustomerGroupId = item.CustomerGroupId;
                result.CreditTermDay = item.CreditTermDay;
                result.CreditAmount = item.CreditAmount;
                result.TaxId = item.TaxId;
                result.NationalCardId = item.NationalCardId;
                result.Content = item.Content;

                result.UpdatedDate = DateTime.UtcNow;
                await context.SaveChangesAsync();
            }

            return result;
        }

        public async Task<List<MEntityAddress>> GetEntityAddressesByEntityIdV2(string entityId)
        {
            return await context!.EntityAddresses!
                .Where(a => a.OrgId!.Equals(orgId) && a.EntityId!.Equals(entityId))
                .OrderByDescending(a => a.CreatedDate)
                .ToListAsync();
        }

        public async Task<MEntityAddress?> GetEntityAddressByIdV2(string addressId)
        {
            Guid id = Guid.Parse(addressId);
            return await context!.EntityAddresses!.Where(a => a.OrgId!.Equals(orgId) && a.Id!.Equals(id)).FirstOrDefaultAsync();
        }

        public async Task<MEntityAddress> AddEntityAddressV2(MEntityAddress address)
        {
            address.Id = Guid.NewGuid();
            address.CreatedDate = DateTime.UtcNow;
            address.UpdatedDate = DateTime.UtcNow;
            address.OrgId = orgId;

            await context!.EntityAddresses!.AddAsync(address);
            await context.SaveChangesAsync();

            return address;
        }

        public async Task<MEntityAddress?> UpdateEntityAddressByIdV2(string addressId, MEntityAddress address)
        {
            Guid id = Guid.Parse(addressId);
            var result = await context!.EntityAddresses!.Where(a => a.OrgId!.Equals(orgId) && a.Id!.Equals(id)).FirstOrDefaultAsync();

            if (result != null)
            {
                result.AddressType = address.AddressType;
                result.Address = address.Address;
                result.UpdatedDate = DateTime.UtcNow;
                await context!.SaveChangesAsync();
            }

            return result;
        }

        public async Task<MEntityAddress?> DeleteEntityAddressByIdV2(string addressId)
        {
            Guid id = Guid.Parse(addressId);
            var result = await context!.EntityAddresses!.Where(a => a.OrgId!.Equals(orgId) && a.Id!.Equals(id)).FirstOrDefaultAsync();

            if (result != null)
            {
                context!.EntityAddresses!.Remove(result);
                await context.SaveChangesAsync();
            }

            return result;
        }
    }
}