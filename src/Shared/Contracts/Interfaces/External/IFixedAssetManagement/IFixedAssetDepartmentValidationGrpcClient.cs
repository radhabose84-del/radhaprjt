using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Contracts.Interfaces.External.IFixedAssetManagement
{
    public interface IFixedAssetDepartmentValidationGrpcClient
    {
         Task<bool> CheckIfDepartmentIsUsedForFixedAssetAsync(int departmentId);
    }
}