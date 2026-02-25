using PartyManagement.Application.Common.Interfaces.IPartyMaster;
using PartyManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using PartyManagement.Infrastructure.Data;

namespace PartyManagement.Infrastructure.Repositories.PartyMaster
{
    public class PartyActivityLogCommandRepository : IPartyActivityLogCommandRepository
    {
         private readonly ApplicationDbContext _applicationDbContext;
        public PartyActivityLogCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<List<PartyActivityLog>> GetActivityLogsByPartyIdAsync(int partyId, CancellationToken cancellationToken)
        {
             return await _applicationDbContext.PartyActivityLog
                .Where(x => x.PartyId == partyId)
                .OrderByDescending(x => x.ChangedOn)
                .ToListAsync(cancellationToken);
        }
        public async Task<int> InsertAsync(PartyActivityLog log, CancellationToken cancellationToken = default)
        {
              await _applicationDbContext.PartyActivityLog.AddAsync(log, cancellationToken);
              return await _applicationDbContext.SaveChangesAsync(cancellationToken);
        }
    }
}