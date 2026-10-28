using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;
using Microsoft.EntityFrameworkCore;

namespace Its.Onix.Api.Database.Repositories
{
    // Fully async (matching the repository below) — per team convention, apply the same
    // pattern to any new API work rather than mixing sync/async in the same call chain.
    public class ServiceRequestRepository : BaseRepository, IServiceRequestRepository
    {
        public ServiceRequestRepository(IDataContext ctx)
        {
            context = ctx;
        }

        public async Task<MServiceRequest> AddServiceRequest(MServiceRequest serviceRequest)
        {
            serviceRequest.Id = Guid.NewGuid();
            serviceRequest.OrgId = orgId;
            serviceRequest.CreatedDate = DateTime.UtcNow;
            serviceRequest.UpdatedDate = null;
            serviceRequest.ApprovedDate = null;

            await context!.ServiceRequests!.AddAsync(serviceRequest);
            await context.SaveChangesAsync();

            return serviceRequest;
        }

        public async Task<MServiceRequest?> GetServiceRequestById(string serviceRequestId)
        {
            var id = Guid.Parse(serviceRequestId);
            return await context!.ServiceRequests!
                .Where(p => p.Id!.Equals(id) && p.OrgId!.Equals(orgId))
                .FirstOrDefaultAsync();
        }

        // ไม่ filter ด้วย orgId เพราะผู้เรียก (ลูกค้าที่ยังไม่มี org) ไม่มี orgId ให้ใช้ —
        // request-id + otp ที่ตรงกันคือสิ่งเดียวที่พิสูจน์ว่ามีสิทธิ์เข้าถึง record นี้
        public async Task<MServiceRequest?> GetServiceRequestByIdAndOtp(string serviceRequestId, string otp)
        {
            var id = Guid.Parse(serviceRequestId);
            return await context!.ServiceRequests!
                .Where(p => p.Id!.Equals(id) && p.Otp!.Equals(otp))
                .FirstOrDefaultAsync();
        }

        public async Task<MServiceRequest?> UpdateServiceRequestById(string serviceRequestId, MServiceRequest serviceRequest)
        {
            var id = Guid.Parse(serviceRequestId);
            var existing = await context!.ServiceRequests!.Where(p => p.Id!.Equals(id)).FirstOrDefaultAsync();
            if (existing == null) return null;

            // Email/Otp/ServiceType/ExpiredDate/OrgId/CreatedDate ไม่ยอมให้แก้ผ่านทางนี้ — เป็น fact
            // ที่ตรึงไว้ตั้งแต่ตอนสร้าง link
            existing.Status = serviceRequest.Status ?? existing.Status;
            existing.Lang = serviceRequest.Lang ?? existing.Lang;
            existing.FirstName = serviceRequest.FirstName;
            existing.LastName = serviceRequest.LastName;
            existing.Phone = serviceRequest.Phone;
            existing.BankCode = serviceRequest.BankCode;
            existing.BankAccountNumber = serviceRequest.BankAccountNumber;
            existing.BankAccountName = serviceRequest.BankAccountName;
            existing.PromptPayId = serviceRequest.PromptPayId;
            existing.BookBankFileDocumentId = serviceRequest.BookBankFileDocumentId ?? existing.BookBankFileDocumentId;
            existing.ShopCode = serviceRequest.ShopCode;
            existing.Username = serviceRequest.Username;
            existing.UpdatedDate = DateTime.UtcNow;

            await context!.SaveChangesAsync();
            return existing;
        }

        public async Task<MServiceRequest?> RegenerateOtp(string serviceRequestId, string newOtp, DateTime newExpiredDate)
        {
            var id = Guid.Parse(serviceRequestId);
            var existing = await context!.ServiceRequests!.Where(p => p.Id!.Equals(id) && p.OrgId!.Equals(orgId)).FirstOrDefaultAsync();
            if (existing == null) return null;

            existing.Otp = newOtp;
            existing.ExpiredDate = newExpiredDate;
            existing.UpdatedDate = DateTime.UtcNow;

            await context!.SaveChangesAsync();
            return existing;
        }

        public async Task<MServiceRequest?> DeleteServiceRequestById(string serviceRequestId)
        {
            var id = Guid.Parse(serviceRequestId);
            var existing = await context!.ServiceRequests!.Where(p => p.Id!.Equals(id) && p.OrgId!.Equals(orgId)).FirstOrDefaultAsync();
            if (existing == null) return null;

            context!.ServiceRequests!.Remove(existing);
            await context.SaveChangesAsync();

            return existing;
        }

        private IQueryable<MServiceRequest> ServiceRequestQuery(VMServiceRequest param)
        {
            var query = context!.ServiceRequests!.Where(p => p.OrgId!.Equals(orgId));

            if (!string.IsNullOrEmpty(param.ServiceType))
            {
                query = query.Where(p => p.ServiceType!.Equals(param.ServiceType));
            }

            if (!string.IsNullOrEmpty(param.Status))
            {
                query = query.Where(p => p.Status!.Equals(param.Status));
            }

            if (!string.IsNullOrEmpty(param.FullTextSearch))
            {
                var q = param.FullTextSearch;
                query = query.Where(p =>
                    p.Email!.Contains(q) ||
                    p.FirstName!.Contains(q) ||
                    p.LastName!.Contains(q) ||
                    p.Phone!.Contains(q) ||
                    p.ShopCode!.Contains(q));
            }

            if (param.FromDate.HasValue)
            {
                query = query.Where(p => p.CreatedDate >= param.FromDate.Value);
            }

            if (param.ToDate.HasValue)
            {
                query = query.Where(p => p.CreatedDate <= param.ToDate.Value);
            }

            return query;
        }

        public async Task<IEnumerable<MServiceRequest>> GetServiceRequests(VMServiceRequest param)
        {
            var offset = param.Offset > 0 ? param.Offset - 1 : 0;
            var limit = param.Limit > 0 ? param.Limit : 0;

            return await ServiceRequestQuery(param)
                .OrderByDescending(p => p.CreatedDate)
                .Skip(offset)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<int> GetServiceRequestCount(VMServiceRequest param)
        {
            return await ServiceRequestQuery(param).CountAsync();
        }
    }
}
