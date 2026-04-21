using Microsoft.EntityFrameworkCore;
using SalesManagement.Application.Common.Interfaces.IComplaintResolution;
using SalesManagement.Infrastructure.Data;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Repositories.ComplaintResolution
{
    public class ComplaintResolutionCommandRepository : IComplaintResolutionCommandRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public ComplaintResolutionCommandRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int> CreateAsync(Domain.Entities.ComplaintResolution entity)
        {
            await _dbContext.ComplaintResolution.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<int> UpdateAsync(Domain.Entities.ComplaintResolution entity)
        {
            var existing = await _dbContext.ComplaintResolution
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

            if (existing == null)
                return 0;

            existing.ResolutionTypeId = entity.ResolutionTypeId;
            existing.ResolutionSummary = entity.ResolutionSummary;
            existing.ReturnQuantity = entity.ReturnQuantity;
            existing.ReturnLocationId = entity.ReturnLocationId > 0 ? entity.ReturnLocationId : null;
            existing.ReturnStatusId = entity.ReturnStatusId;
            existing.CreditAmount = entity.CreditAmount;
            existing.FinanceReference = entity.FinanceReference;
            existing.ReplacementQuantity = entity.ReplacementQuantity;
            existing.DispatchReference = entity.DispatchReference;
            existing.ActionDescription = entity.ActionDescription;
            existing.ClosureStatusId = entity.ClosureStatusId;
            existing.ClosureRemarks = entity.ClosureRemarks;
            existing.IsActive = entity.IsActive;

            // First-time resolver stamp (from SubmitResolution upsert path) — never overwrite existing stamp
            if (existing.ResolvedBy == null && entity.ResolvedBy.HasValue)
            {
                existing.ResolvedBy = entity.ResolvedBy;
                existing.ResolvedDate = entity.ResolvedDate;
            }

            // Set closed audit fields if provided
            if (entity.ClosedBy.HasValue)
            {
                existing.ClosedBy = entity.ClosedBy;
                existing.ClosedDate = entity.ClosedDate;
            }

            await _dbContext.SaveChangesAsync();
            return existing.Id;
        }
    }
}
