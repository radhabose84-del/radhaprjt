 using System.Collections.Generic;
 using System.Threading.Tasks;
 using Contracts.Dtos.Maintenance;

 namespace Contracts.Interfaces.External.IUser
 {
     public interface IDepartmentGrpcClient
     {
          Task<List<DepartmentDto>> GetAllDepartmentAsync();
     }
 }