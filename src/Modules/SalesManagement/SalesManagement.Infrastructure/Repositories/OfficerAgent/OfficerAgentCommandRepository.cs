using Microsoft.EntityFrameworkCore;
using SalesManagement.Application.Common.Interfaces.IOfficerAgent;
using SalesManagement.Infrastructure.Data;

namespace SalesManagement.Infrastructure.Repositories.OfficerAgent
{
    public class OfficerAgentCommandRepository : IOfficerAgentCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public OfficerAgentCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<int> CreateAsync(Domain.Entities.OfficerAgent entity)
        {
            await _applicationDbContext.OfficerAgent.AddAsync(entity);
            await _applicationDbContext.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<int> UpdateAsync(Domain.Entities.OfficerAgent entity)
        {
            var existing = await _applicationDbContext.OfficerAgent
                .FirstOrDefaultAsync(x => x.Id == entity.Id);

            if (existing == null)
                return 0;

            existing.AgentId = entity.AgentId;
            existing.MarketingOfficerId = entity.MarketingOfficerId;
            existing.ValidityFrom = entity.ValidityFrom;
            existing.ValidityTo = entity.ValidityTo;
            existing.IsActive = entity.IsActive;

            _applicationDbContext.OfficerAgent.Update(existing);
            await _applicationDbContext.SaveChangesAsync();
            return existing.Id;
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct)
        {
            var existing = await _applicationDbContext.OfficerAgent
                .FirstOrDefaultAsync(x => x.Id == id, ct);

            if (existing == null)
                return false;

            _applicationDbContext.OfficerAgent.Remove(existing);
            await _applicationDbContext.SaveChangesAsync(ct);
            return true;
        }
    }
}
