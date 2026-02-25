using Contracts.Dtos.Users;

namespace Contracts.Interfaces.External.IUser
{
    public interface IDepartmentAllGrpcClient
    {
        Task<List<DepartmentAllDto>> GetDepartmentAllAsync();

    }
}