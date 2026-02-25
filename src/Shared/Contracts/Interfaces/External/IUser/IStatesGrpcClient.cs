using Contracts.Dtos.Users;

namespace Contracts.Interfaces.External.IUser
{
    public interface IStatesGrpcClient
    {
        Task<List<StatesDto>> GetAllStateAsync();
    }
}