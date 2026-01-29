using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MaintenanceManagement.Application.Common.IMachineSpecification
{
    public interface IMachineSpecificationCommandRepository
    {
        Task<int> CreateAsync(MaintenanceManagement.Domain.Entities.MachineSpecification machineSpecification);
        Task<int> DeleteAsync(int Id, MaintenanceManagement.Domain.Entities.MachineSpecification machineSpecification);
        Task<bool> IsDuplicateSpecificationAsync(int machineId, int specificationId);
        Task<bool> UpdateAsync(List<MaintenanceManagement.Domain.Entities.MachineSpecification> specifications);
    }
}