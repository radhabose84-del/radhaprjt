using FinanceManagement.Application.Common.Interfaces.ITaxCode;
using FinanceManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.Infrastructure.Repositories.TaxCode
{
    public class TaxCodeCommandRepository : ITaxCodeCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public TaxCodeCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        // ─── Tax Code Master ───────────────────────────────────────────────
        public async Task<int> CreateTaxCodeAsync(Domain.Entities.TaxCodeMaster entity)
        {
            await _applicationDbContext.TaxCodeMaster.AddAsync(entity);
            await _applicationDbContext.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<int> UpdateTaxCodeAsync(Domain.Entities.TaxCodeMaster entity)
        {
            var existing = await _applicationDbContext.TaxCodeMaster
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

            if (existing == null)
                return 0;

            // TaxCode and TaxType are immutable.
            existing.TaxName = entity.TaxName;
            existing.TaxComponent = entity.TaxComponent;
            existing.Direction = entity.Direction;
            existing.StatutorySection = entity.StatutorySection;
            existing.ThresholdAmount = entity.ThresholdAmount;
            existing.ThresholdAggregate = entity.ThresholdAggregate;
            existing.HsnSacCode = entity.HsnSacCode;
            existing.IsSystemOnlyPosting = entity.IsSystemOnlyPosting;
            existing.IsEefcRelevant = entity.IsEefcRelevant;
            existing.IsStatutoryFixed = entity.IsStatutoryFixed;
            existing.IsActive = entity.IsActive;

            _applicationDbContext.TaxCodeMaster.Update(existing);
            await _applicationDbContext.SaveChangesAsync();
            return existing.Id;
        }

        public async Task<bool> SoftDeleteTaxCodeAsync(int id, CancellationToken ct)
        {
            var existing = await _applicationDbContext.TaxCodeMaster
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            existing.IsDeleted = IsDelete.Deleted;
            _applicationDbContext.TaxCodeMaster.Update(existing);
            await _applicationDbContext.SaveChangesAsync(ct);
            return true;
        }

        // Closes the prior open version and inserts a new one with the next VersionNo (AC3-A).
        public async Task<int> CreateRateVersionAsync(Domain.Entities.TaxCodeRateVersion entity)
        {
            var existingVersions = await _applicationDbContext.TaxCodeRateVersion
                .Where(v => v.TaxCodeId == entity.TaxCodeId && v.IsDeleted == IsDelete.NotDeleted)
                .ToListAsync();

            var openVersion = existingVersions.FirstOrDefault(v => v.EffectiveTo == null);
            if (openVersion != null && openVersion.EffectiveFrom < entity.EffectiveFrom)
            {
                openVersion.EffectiveTo = entity.EffectiveFrom.AddDays(-1);
                _applicationDbContext.TaxCodeRateVersion.Update(openVersion);
            }

            entity.VersionNo = existingVersions.Count == 0 ? 1 : existingVersions.Max(v => v.VersionNo) + 1;

            await _applicationDbContext.TaxCodeRateVersion.AddAsync(entity);
            await _applicationDbContext.SaveChangesAsync();
            return entity.Id;
        }

        // ─── Tax Account Linkage ───────────────────────────────────────────
        public async Task<int> CreateLinkageAsync(Domain.Entities.TaxAccountLinkage entity)
        {
            await _applicationDbContext.TaxAccountLinkage.AddAsync(entity);
            await _applicationDbContext.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<bool> ActivateLinkageAsync(int id, CancellationToken ct)
        {
            var existing = await _applicationDbContext.TaxAccountLinkage
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            existing.IsActivated = true;
            existing.ApprovalStatus = "APPROVED";
            _applicationDbContext.TaxAccountLinkage.Update(existing);
            await _applicationDbContext.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> SoftDeleteLinkageAsync(int id, CancellationToken ct)
        {
            var existing = await _applicationDbContext.TaxAccountLinkage
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            existing.IsDeleted = IsDelete.Deleted;
            _applicationDbContext.TaxAccountLinkage.Update(existing);
            await _applicationDbContext.SaveChangesAsync(ct);
            return true;
        }

        // ─── GSTR Section Mapping ──────────────────────────────────────────
        public async Task<int> CreateGstrMappingAsync(Domain.Entities.GstrSectionMapping entity)
        {
            await _applicationDbContext.GstrSectionMapping.AddAsync(entity);
            await _applicationDbContext.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<int> UpdateGstrMappingAsync(Domain.Entities.GstrSectionMapping entity)
        {
            var existing = await _applicationDbContext.GstrSectionMapping
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

            if (existing == null)
                return 0;

            existing.GstrType = entity.GstrType;
            existing.SectionCode = entity.SectionCode;
            existing.SectionName = entity.SectionName;
            existing.AccountRangeFrom = entity.AccountRangeFrom;
            existing.AccountRangeTo = entity.AccountRangeTo;
            existing.TolerancePercent = entity.TolerancePercent;
            existing.IsActive = entity.IsActive;

            _applicationDbContext.GstrSectionMapping.Update(existing);
            await _applicationDbContext.SaveChangesAsync();
            return existing.Id;
        }

        public async Task<bool> SoftDeleteGstrMappingAsync(int id, CancellationToken ct)
        {
            var existing = await _applicationDbContext.GstrSectionMapping
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            existing.IsDeleted = IsDelete.Deleted;
            _applicationDbContext.GstrSectionMapping.Update(existing);
            await _applicationDbContext.SaveChangesAsync(ct);
            return true;
        }
    }
}
