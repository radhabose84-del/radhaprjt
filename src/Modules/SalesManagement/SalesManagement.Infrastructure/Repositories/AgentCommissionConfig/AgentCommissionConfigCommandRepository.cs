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
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

            if (existingEntity == null)
                return 0;

            existingEntity.AgentId = entity.AgentId;
            existingEntity.SalesSegmentId = entity.SalesSegmentId;
            existingEntity.ItemId = entity.ItemId;
            existingEntity.CommissionTypeId = entity.CommissionTypeId;
            existingEntity.UomId = entity.UomId;
            existingEntity.CommissionPercentage = entity.CommissionPercentage;
            existingEntity.CurrencyId = entity.CurrencyId;
            existingEntity.SubAgentPercentage = entity.SubAgentPercentage;
            existingEntity.ValidityFrom = entity.ValidityFrom;
            existingEntity.ValidityTo = entity.ValidityTo;
            existingEntity.IsActive = entity.IsActive;

            _applicationDbContext.AgentCommissionConfig.Update(existingEntity);
            await _applicationDbContext.SaveChangesAsync();
            return existingEntity.Id;
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
        {
            var existing = await _applicationDbContext.AgentCommissionConfig
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            existing.IsDeleted = IsDelete.Deleted;
            _applicationDbContext.AgentCommissionConfig.Update(existing);
            await _applicationDbContext.SaveChangesAsync(ct);
            return true;
        }
    }
}
