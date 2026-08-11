using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.Models;
using Its.Onix.Api.Utils;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Services
{
    public class DocumentNumberService : BaseService, IDocumentNumberService
    {
        private readonly IDocumentNumberRepository _repo;
        private readonly IRedisHelper _redis;

        public DocumentNumberService(IDocumentNumberRepository repo, IRedisHelper redis) : base()
        {
            _repo = repo;
            _redis = redis;
        }

        public async Task<MDocumentNumberConfig> AddDocumentNumberConfig(string orgId, MDocumentNumberConfig config)
        {
            _repo.SetCustomOrgId(orgId);

            var exists = await _repo.IsDocumentTypeExistAsync(config.DocumentType!);
            if (exists)
                throw new InvalidOperationException($"DocumentType '{config.DocumentType}' already exists in this organization");

            return await _repo.AddDocumentNumberConfigAsync(config);
        }

        public async Task<MDocumentNumberConfig?> GetDocumentNumberConfigById(string orgId, string id)
        {
            _repo.SetCustomOrgId(orgId);
            return await _repo.GetDocumentNumberConfigByIdAsync(id);
        }

        public async Task<List<MDocumentNumberConfig>> GetDocumentNumberConfigs(string orgId, VMDocumentNumberConfig param)
        {
            _repo.SetCustomOrgId(orgId);
            return await _repo.GetDocumentNumberConfigsAsync(param);
        }

        public async Task<int> GetDocumentNumberConfigCount(string orgId, VMDocumentNumberConfig param)
        {
            _repo.SetCustomOrgId(orgId);
            return await _repo.GetDocumentNumberConfigCountAsync(param);
        }

        public async Task<MDocumentNumberConfig?> UpdateDocumentNumberConfigById(string orgId, string id, MDocumentNumberConfig config)
        {
            _repo.SetCustomOrgId(orgId);
            return await _repo.UpdateDocumentNumberConfigByIdAsync(id, config);
        }

        public async Task<MDocumentNumberConfig?> DeleteDocumentNumberConfigById(string orgId, string id)
        {
            _repo.SetCustomOrgId(orgId);
            return await _repo.DeleteDocumentNumberConfigByIdAsync(id);
        }

        public async Task<string> GetDocumentNumber(string orgId, string documentType)
        {
            _repo.SetCustomOrgId(orgId);

            var lockKey = $"lock:DocumentNumber:{orgId}:{documentType}";
            using var redLock = await _redis.AcquireRedLockAsync(lockKey, TimeSpan.FromSeconds(10));

            if (!redLock.IsAcquired)
                throw new InvalidOperationException($"Unable to acquire lock for DocumentType '{documentType}'");

            var config = await _repo.GetDocumentNumberConfigByTypeAsync(documentType);
            if (config == null)
                throw new KeyNotFoundException($"DocumentType '{documentType}' not found");

            var now = DateTime.UtcNow;

            // Compute the reset key: "2026" for Yearly, "2026-08" for Monthly
            var currentKey = (config.ResetType == "Yearly")
                ? now.Year.ToString()
                : $"{now.Year}-{now.Month:D2}";

            // Reset seq if we crossed a month/year boundary
            var seqNo = (config.CurrentSequenceKey != currentKey) ? 0 : (config.CurrentSequenceNo ?? 0);
            seqNo++;

            await _repo.UpdateDocumentNumberSequenceAsync(config.Id!.ToString()!, seqNo, currentKey);

            // Format: apply YearOffset, pad seq to SeqDigit digits
            var year = now.Year + (config.YearOffset ?? 0);
            var seqStr = seqNo.ToString().PadLeft(config.SeqDigit ?? 4, '0');

            var docNo = (config.DocumentFormat ?? "")
                .Replace("${yyyy}", year.ToString())
                .Replace("${mm}", now.Month.ToString())
                .Replace("${seq}", seqStr);

            return docNo;
        }

        public async Task<MDocumentNumberConfig?> UpdateDocumentNumberSequence(string orgId, string id, int sequenceNo, string sequenceKey)
        {
            _repo.SetCustomOrgId(orgId);

            // Fetch first to resolve documentType for lock key
            var existing = await _repo.GetDocumentNumberConfigByIdAsync(id);
            if (existing == null) return null;

            var lockKey = $"lock:DocumentNumber:{orgId}:{existing.DocumentType}";
            using var redLock = await _redis.AcquireRedLockAsync(lockKey, TimeSpan.FromSeconds(10));

            if (!redLock.IsAcquired)
                throw new InvalidOperationException($"Unable to acquire lock for DocumentType '{existing.DocumentType}'");

            return await _repo.UpdateDocumentNumberSequenceAsync(id, sequenceNo, sequenceKey);
        }
    }
}
