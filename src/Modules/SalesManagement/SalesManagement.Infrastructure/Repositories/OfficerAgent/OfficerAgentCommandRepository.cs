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

        public async Task<int> CreateBatchAsync(List<Domain.Entities.OfficerAgent> entities)
        {
            await _applicationDbContext.OfficerAgent.AddRangeAsync(entities);
            await _applicationDbContext.SaveChangesAsync();
            return entities.Count;
        }

        public async Task<int> UpdateBatchAsync(List<Domain.Entities.OfficerAgent> entities)
        {
            var ids = entities.Select(e => e.Id).ToList();

            var existingList = await _applicationDbContext.OfficerAgent
                .Where(x => ids.Contains(x.Id))
                .ToListAsync();

            var existingDict = existingList.ToDictionary(x => x.Id);

            foreach (var entity in entities)
            {
                if (!existingDict.TryGetValue(entity.Id, out var existing))
                    continue;

                existing.AgentId = entity.AgentId;
                existing.MarketingOfficerId = entity.MarketingOfficerId;
                existing.ValidityFrom = entity.ValidityFrom;
                existing.ValidityTo = entity.ValidityTo;
                existing.IsActive = entity.IsActive;
            }

            await _applicationDbContext.SaveChangesAsync();
            return existingList.Count;
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
