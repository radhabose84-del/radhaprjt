using Contracts.Dtos.Users;

namespace Contracts.Interfaces.External.IUser
{
    public interface ICountryGrpcClient
    {
        Task<List<CountryDto>> GetAllCountryAsync();
    }
}