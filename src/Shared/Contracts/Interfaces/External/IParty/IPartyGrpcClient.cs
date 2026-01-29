using System.Threading;
using System.Threading.Tasks;
using Contracts.Dtos.Party;

namespace Contracts.Interfaces.External.IParty
{
    public interface IPartyGrpcClient
    {
        Task<PartyDetailsDto> GetPartyByIdAsync(int id);
        //Task<PartyDto> GetPartiesPagedAsync(int page, int size, string? search, CancellationToken ct);
    }
}