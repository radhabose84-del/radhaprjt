using Contracts.Dtos.Users;

namespace Contracts.Interfaces.External.IUser
{
    public interface ICityGrpcClient
    {
         Task<List<CityDto>> GetAllCityAsync();
    }
}