using Contracts.Dtos.Maintenance;

namespace Contracts.Interfaces.External.IUser
{
    public interface IDepartmentGrpcClient
     {
          Task<List<DepartmentDto>> GetAllDepartmentAsync();
     }
 }