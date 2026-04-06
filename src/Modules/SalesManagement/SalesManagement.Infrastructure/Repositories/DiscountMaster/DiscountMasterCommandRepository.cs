using Microsoft.EntityFrameworkCore;
using SalesManagement.Application.Common.Interfaces.IDiscountMaster;
using SalesManagement.Domain.Entities;
using SalesManagement.Infrastructure.Data;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Repositories.DiscountMaster
{
    public class DiscountMasterCommandRepository : IDiscountMasterCommandRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public DiscountMasterCommandRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int> CreateAsync(Domain.Entities.DiscountMaster entity)
        {
            // Auto-generate DiscountCode
            var lastId = await _dbContext.DiscountMaster
                .OrderByDescending(x => x.Id)
                .Select(x => x.Id)
                .FirstOrDefaultAsync();

            entity.DiscountCode = $"DC{(lastId + 1):D5}";

            _dbContext.DiscountMaster.Add(entity);
            await _dbContext.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<int> UpdateAsync(Domain.Entities.DiscountMaster entity)
        {
            var existing = await _dbContext.DiscountMaster
                .Include(d => d.DiscountSlabs)
                .Include(d => d.DiscountSalesGroups)
                .Include(d => d.DiscountPaymentTerms)
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

            if (existing == null)
                return 0;

            // Update header fields (DiscountCode is immutable — not updated)
            existing.DiscountName = entity.DiscountName;
            existing.DiscountTypeId = entity.DiscountTypeId;
            existing.ApplicableLevelId = entity.ApplicableLevelId;
            existing.TriggerEventId = entity.TriggerEventId;
            existing.RequiresApproval = entity.RequiresApproval;
            existing.MaxDiscountLimitTypeId = entity.MaxDiscountLimitTypeId;
            existing.ValueTypeId = entity.ValueTypeId;
            existing.DiscountValue = entity.DiscountValue;
            existing.SlabTypeId = entity.SlabTypeId;
            existing.IsActive = entity.IsActive;

            // Replace strategy: remove old children, add new ones
            if (existing.DiscountSlabs != null)
                _dbContext.DiscountSlab.RemoveRange(existing.DiscountSlabs);

            if (existing.DiscountSalesGroups != null)
                _dbContext.DiscountSalesGroup.RemoveRange(existing.DiscountSalesGroups);

            if (existing.DiscountPaymentTerms != null)
                _dbContext.DiscountPaymentTerm.RemoveRange(existing.DiscountPaymentTerms);

            // Add new children
            if (entity.DiscountSlabs != null && entity.DiscountSlabs.Count > 0)
            {
                foreach (var slab in entity.DiscountSlabs)
                {
                    slab.DiscountMasterId = existing.Id;
                    _dbContext.DiscountSlab.Add(slab);
                }
            }

            if (entity.DiscountSalesGroups != null && entity.DiscountSalesGroups.Count > 0)
            {
                foreach (var sg in entity.DiscountSalesGroups)
                {
                    sg.DiscountMasterId = existing.Id;
                    _dbContext.DiscountSalesGroup.Add(sg);
                }
            }

            if (entity.DiscountPaymentTerms != null && entity.DiscountPaymentTerms.Count > 0)
            {
                foreach (var pt in entity.DiscountPaymentTerms)
                {
                    pt.DiscountMasterId = existing.Id;
                    _dbContext.DiscountPaymentTerm.Add(pt);
                }
            }

            _dbContext.DiscountMaster.Update(existing);
            await _dbContext.SaveChangesAsync();
            return existing.Id;
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
        {
            var existing = await _dbContext.DiscountMaster
                .Include(d => d.DiscountSlabs)
                .Include(d => d.DiscountSalesGroups)
                .Include(d => d.DiscountPaymentTerms)
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            existing.IsDeleted = IsDelete.Deleted;

            // Soft-delete child records
            if (existing.DiscountSlabs != null)
            {
                foreach (var slab in existing.DiscountSlabs)
                    slab.IsDeleted = IsDelete.Deleted;
            }

            if (existing.DiscountSalesGroups != null)
            {
                foreach (var sg in existing.DiscountSalesGroups)
                    sg.IsDeleted = IsDelete.Deleted;
            }

            if (existing.DiscountPaymentTerms != null)
            {
                foreach (var pt in existing.DiscountPaymentTerms)
                    pt.IsDeleted = IsDelete.Deleted;
            }

            _dbContext.DiscountMaster.Update(existing);
            await _dbContext.SaveChangesAsync(ct);
            return true;
        }
    }
}
