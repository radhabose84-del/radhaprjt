using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.HttpResponse;
using MediatR;
using static MaintenanceManagement.Domain.Common.BaseEntity;

namespace MaintenanceManagement.Application.MachineGroup.Command.UpdateMachineGroup
{
    public class UpdateMachineGroupCommand : IRequest<bool>
    {
        public int Id { get; set; }
        public string? GroupName { get; set; }
        public int Manufacturer { get; set; }
        public int UnitId { get; set; }
        public int DepartmentId { get; set; }
        public byte IsActive { get; set; }
        public byte PowerSource { get; set; } 
        
    }
}