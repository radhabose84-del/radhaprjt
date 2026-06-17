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
                .FirstOrDefaultAsync(x => x.Id == entity.Id);

            if (existing == null)
                return 0;

            // TaxCode and TaxTypeId are immutable.
            existing.TaxName = entity.TaxName;
            existing.TaxComponentId = entity.TaxComponentId;
            existing.DirectionId = entity.DirectionId;
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

        // Closes the prior open version and inserts a new one with the next VersionNo (AC3-A).
        public async Task<int> CreateRateVersionAsync(Domain.Entities.TaxCodeRateVersion entity)
        {
            var existingVersions = await _applicationDbContext.TaxCodeRateVersion
                .Where(v => v.TaxCodeId == entity.TaxCodeId)
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

        // Approval-complete: activate the PENDING row (IsActive=Active + APPROVED) and close the
        // prior active row for the same GL account (IsActive=Inactive + EffectiveTo) so there is
        // always exactly one active linkage per account. The closed row's change is captured by ActivityLog.
        public async Task<bool> ActivateLinkageAsync(int id, int approvedStatusId, CancellationToken ct)
        {
            var target = await _applicationDbContext.TaxAccountLinkage
                .FirstOrDefaultAsync(x => x.Id == id, ct);

            if (target == null)
                return false;

            var prior = await _applicationDbContext.TaxAccountLinkage
                .Where(x => x.Id != target.Id
                    && x.CompanyId == target.CompanyId
                    && x.GlAccountId == target.GlAccountId
                    && x.IsActive == Status.Active
                    && x.EffectiveTo == null)
                .ToListAsync(ct);

            foreach (var row in prior)
            {
                row.IsActive = Status.Inactive;
                row.EffectiveTo = target.EffectiveFrom.AddDays(-1);
                _applicationDbContext.TaxAccountLinkage.Update(row);
            }

            target.IsActive = Status.Active;
            target.StatusId = approvedStatusId;
            _applicationDbContext.TaxAccountLinkage.Update(target);

            await _applicationDbContext.SaveChangesAsync(ct);
            return true;
        }
    }
}
