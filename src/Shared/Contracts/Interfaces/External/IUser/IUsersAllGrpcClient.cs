using Contracts.Dtos.Users;

namespace Contracts.Interfaces.External.IUser
{
    public interface IUsersAllGrpcClient
    {
        Task<List<UsersAllDto>> GetUserAllAsync();
    }
}