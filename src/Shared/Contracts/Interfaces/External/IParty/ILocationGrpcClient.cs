using Contracts.Dtos.Party;

namespace Contracts.Interfaces.External.IParty
{
    public interface ILocationGrpcClient
    {
        Task<LocationDto?> GetOrCreateLocationAsync(string city, string state, string country);
    }
}