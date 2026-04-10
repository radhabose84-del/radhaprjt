using Microsoft.EntityFrameworkCore;
using SalesManagement.Application.Common.Interfaces.IAgentCommissionConfig;
using SalesManagement.Infrastructure.Data;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Repositories.AgentCommissionConfig
{
    public class AgentCommissionConfigCommandRepository : IAgentCommissionConfigCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public AgentCommissionConfigCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<int> CreateAsync(Domain.Entities.AgentCommissionConfig entity)
        {
            await _applicationDbContext.AgentCommissionConfig.AddAsync(entity);
            await _applicationDbContext.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<int> UpdateAsync(Domain.Entities.AgentCommissionConfig entity)
        {
            var existingEntity = await _applicationDbContext.AgentCommissionConfig
                .Include(x => x.AgentCommissionSalesGroups)
                .Include(x => x.AgentCommissionPaymentTerms)
                .Include(x => x.AgentCommissionSlabs)
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

            if (existingEntity == null)
                return 0;

            // Update header fields
            existingEntity.AgentId = entity.AgentId;
            existingEntity.CommissionTypeId = entity.CommissionTypeId;
            existingEntity.CommissionBasisId = entity.CommissionBasisId;
            existingEntity.ApplicableLevelId = entity.ApplicableLevelId;
            existingEntity.CommissionPercentage = entity.CommissionPercentage;
            existingEntity.ValidityFrom = entity.ValidityFrom;
            existingEntity.ValidityTo = entity.ValidityTo;
            existingEntity.TriggerEventId = entity.TriggerEventId;
            existingEntity.SlabTypeId = entity.SlabTypeId;
            existingEntity.CommissionSplitId = entity.CommissionSplitId;
            existingEntity.IsActive = entity.IsActive;

            // Replace strategy: remove old children, add new ones

            // SalesGroups
            if (existingEntity.AgentCommissionSalesGroups != null)
                _applicationDbContext.AgentCommissionSalesGroup.RemoveRange(existingEntity.AgentCommissionSalesGroups);

            if (entity.AgentCommissionSalesGroups != null && entity.AgentCommissionSalesGroups.Any())
            {
                foreach (var sg in entity.AgentCommissionSalesGroups)
                    sg.AgentCommissionConfigId = existingEntity.Id;
                await _applicationDbContext.AgentCommissionSalesGroup.AddRangeAsync(entity.AgentCommissionSalesGroups);
            }

            // PaymentTerms
            if (existingEntity.AgentCommissionPaymentTerms != null)
                _applicationDbContext.AgentCommissionPaymentTerm.RemoveRange(existingEntity.AgentCommissionPaymentTerms);

            if (entity.AgentCommissionPaymentTerms != null && entity.AgentCommissionPaymentTerms.Any())
            {
                foreach (var pt in entity.AgentCommissionPaymentTerms)
                    pt.AgentCommissionConfigId = existingEntity.Id;
                await _applicationDbContext.AgentCommissionPaymentTerm.AddRangeAsync(entity.AgentCommissionPaymentTerms);
            }

            // Slabs
            if (existingEntity.AgentCommissionSlabs != null)
                _applicationDbContext.AgentCommissionSlab.RemoveRange(existingEntity.AgentCommissionSlabs);

            if (entity.AgentCommissionSlabs != null && entity.AgentCommissionSlabs.Any())
            {
                foreach (var slab in entity.AgentCommissionSlabs)
                    slab.AgentCommissionConfigId = existingEntity.Id;
                await _applicationDbContext.AgentCommissionSlab.AddRangeAsync(entity.AgentCommissionSlabs);
            }

            _applicationDbContext.AgentCommissionConfig.Update(existingEntity);
            await _applicationDbContext.SaveChangesAsync();
            return existingEntity.Id;
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
        {
            var existing = await _applicationDbContext.AgentCommissionConfig
                .Include(x => x.AgentCommissionSalesGroups)
                .Include(x => x.AgentCommissionPaymentTerms)
                .Include(x => x.AgentCommissionSlabs)
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            existing.IsDeleted = IsDelete.Deleted;

            // Soft-delete children
            if (existing.AgentCommissionSalesGroups != null)
                foreach (var sg in existing.AgentCommissionSalesGroups)
                    sg.IsDeleted = IsDelete.Deleted;

            if (existing.AgentCommissionPaymentTerms != null)
                foreach (var pt in existing.AgentCommissionPaymentTerms)
                    pt.IsDeleted = IsDelete.Deleted;

            if (existing.AgentCommissionSlabs != null)
                foreach (var slab in existing.AgentCommissionSlabs)
                    slab.IsDeleted = IsDelete.Deleted;

            _applicationDbContext.AgentCommissionConfig.Update(existing);
            await _applicationDbContext.SaveChangesAsync(ct);
            return true;
        }
    }
}
