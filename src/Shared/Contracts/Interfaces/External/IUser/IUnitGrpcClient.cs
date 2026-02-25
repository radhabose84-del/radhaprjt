using Contracts.Dtos.Maintenance;

namespace Contracts.Interfaces.External.IUser
{
    public interface IUnitGrpcClient
    {
        Task<List<UnitDto>> GetAllUnitAsync();
        Task<List<UnitDto>> GetUserUnitAsync(int userId);
    }
}