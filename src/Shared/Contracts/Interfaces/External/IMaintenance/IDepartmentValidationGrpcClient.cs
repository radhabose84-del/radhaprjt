using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Contracts.Interfaces.External.IMaintenance
{
    public interface IDepartmentValidationGrpcClient
    {
        Task<bool> CheckIfDepartmentIsUsedAsync(int departmentId);
    }
}