using Microsoft.EntityFrameworkCore;
using SalesManagement.Application.Common.Interfaces.IAgentCustomerMapping;
using SalesManagement.Infrastructure.Data;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Repositories.AgentCustomerMapping
{
    public class AgentCustomerMappingCommandRepository : IAgentCustomerMappingCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public AgentCustomerMappingCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<int> CreateAsync(Domain.Entities.AgentCustomerMapping entity)
        {
            // BR-1: If this mapping is marked as Default Agent, clear any existing default for the same customer
            if (entity.IsDefaultAgent)
            {
                var existingDefaults = await _applicationDbContext.AgentCustomerMapping
                    .Where(x => x.CustomerId == entity.CustomerId
                             && x.IsDefaultAgent
                             && x.IsDeleted == IsDelete.NotDeleted)
                    .ToListAsync();

                foreach (var existing in existingDefaults)
                {
                    existing.IsDefaultAgent = false;
                    _applicationDbContext.AgentCustomerMapping.Update(existing);
                }
            }

            await _applicationDbContext.AgentCustomerMapping.AddAsync(entity);
            await _applicationDbContext.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<int> UpdateAsync(Domain.Entities.AgentCustomerMapping entity)
        {
            var existingEntity = await _applicationDbContext.AgentCustomerMapping
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

            if (existingEntity == null)
                return 0;

            // BR-1: If this mapping is marked as Default Agent, clear any existing default for the same customer (excluding self)
            if (entity.IsDefaultAgent)
            {
                var existingDefaults = await _applicationDbContext.AgentCustomerMapping
                    .Where(x => x.CustomerId == existingEntity.CustomerId
                             && x.IsDefaultAgent
                             && x.IsDeleted == IsDelete.NotDeleted
                             && x.Id != entity.Id)
                    .ToListAsync();

                foreach (var other in existingDefaults)
                {
                    other.IsDefaultAgent = false;
                    _applicationDbContext.AgentCustomerMapping.Update(other);
                }
            }

            existingEntity.AgentId = entity.AgentId;
            existingEntity.SubAgentId = entity.SubAgentId;
            existingEntity.SalesGroupId = entity.SalesGroupId;
            existingEntity.EffectiveFrom = entity.EffectiveFrom;
            existingEntity.EffectiveTo = entity.EffectiveTo;
            existingEntity.IsDefaultAgent = entity.IsDefaultAgent;
            existingEntity.Remarks = entity.Remarks;
            existingEntity.IsActive = entity.IsActive;

            _applicationDbContext.AgentCustomerMapping.Update(existingEntity);
            await _applicationDbContext.SaveChangesAsync();
            return existingEntity.Id;
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
        {
            var existing = await _applicationDbContext.AgentCustomerMapping
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            existing.IsDeleted = IsDelete.Deleted;
            _applicationDbContext.AgentCustomerMapping.Update(existing);
            await _applicationDbContext.SaveChangesAsync(ct);
            return true;
        }
    }
}
