using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.MachineSpecification.Command;

namespace MaintenanceManagement.Application.Common.Interfaces.IMachineSpecification
{
    public interface IMachineSpecificationQueryRepository
    {
        Task<List<MachineSpecificationDto>> GetByIdAsync(int Id);
        Task<MachineSpecificationDto?> GetBySpecificationIdAsync(int Id);

    }
}