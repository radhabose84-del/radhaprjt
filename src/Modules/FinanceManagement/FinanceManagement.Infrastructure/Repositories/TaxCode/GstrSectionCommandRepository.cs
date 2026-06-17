using FinanceManagement.Application.Common.Interfaces.IGstrSection;
using FinanceManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FinanceManagement.Infrastructure.Repositories.TaxCode
{
    public class GstrSectionCommandRepository : IGstrSectionCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public GstrSectionCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        // ─── Section Master ────────────────────────────────────────────────
        public async Task<int> CreateSectionAsync(Domain.Entities.GstrSectionMaster entity)
        {
            await _applicationDbContext.GstrSectionMaster.AddAsync(entity);
            await _applicationDbContext.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<int> UpdateSectionAsync(Domain.Entities.GstrSectionMaster entity)
        {
            var existing = await _applicationDbContext.GstrSectionMaster.FirstOrDefaultAsync(x => x.Id == entity.Id);
            if (existing == null)
                return 0;

            // ReportType + SectionCode are immutable; only the name + active flag change.
            existing.SectionName = entity.SectionName;
            existing.IsActive = entity.IsActive;

            _applicationDbContext.GstrSectionMaster.Update(existing);
            await _applicationDbContext.SaveChangesAsync();
            return existing.Id;
        }

        public async Task<bool> DeleteSectionAsync(int id, CancellationToken ct)
        {
            var existing = await _applicationDbContext.GstrSectionMaster.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (existing == null)
                return false;

            _applicationDbContext.GstrSectionMaster.Remove(existing);
            await _applicationDbContext.SaveChangesAsync(ct);
            return true;
        }

        // ─── Section Account Linkage ───────────────────────────────────────
        public async Task<int> CreateLinkageAsync(Domain.Entities.GstrSectionAccountLinkage entity)
        {
            await _applicationDbContext.GstrSectionAccountLinkage.AddAsync(entity);
            await _applicationDbContext.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<int> UpdateLinkageAsync(Domain.Entities.GstrSectionAccountLinkage entity)
        {
            var existing = await _applicationDbContext.GstrSectionAccountLinkage.FirstOrDefaultAsync(x => x.Id == entity.Id);
            if (existing == null)
                return 0;

            existing.SectionMasterId = entity.SectionMasterId;
            existing.AccountRangeFrom = entity.AccountRangeFrom;
            existing.AccountRangeTo = entity.AccountRangeTo;
            existing.DerivedValue = entity.DerivedValue;
            existing.ExpectedValue = entity.ExpectedValue;
            existing.TolerancePercent = entity.TolerancePercent;
            existing.IsActive = entity.IsActive;

            _applicationDbContext.GstrSectionAccountLinkage.Update(existing);
            await _applicationDbContext.SaveChangesAsync();
            return existing.Id;
        }

        public async Task<bool> DeleteLinkageAsync(int id, CancellationToken ct)
        {
            var existing = await _applicationDbContext.GstrSectionAccountLinkage.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (existing == null)
                return false;

            _applicationDbContext.GstrSectionAccountLinkage.Remove(existing);
            await _applicationDbContext.SaveChangesAsync(ct);
            return true;
        }
    }
}
