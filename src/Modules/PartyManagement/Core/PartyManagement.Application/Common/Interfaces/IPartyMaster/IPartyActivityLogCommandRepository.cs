using PartyManagement.Domain.Entities;

namespace PartyManagement.Application.Common.Interfaces.IPartyMaster
{
    public interface IPartyActivityLogCommandRepository
    {
        Task<int> InsertAsync(PartyActivityLog log, CancellationToken cancellationToken = default);
        Task<List<PartyActivityLog>> GetActivityLogsByPartyIdAsync(int partyId, CancellationToken cancellationToken);
    }
}