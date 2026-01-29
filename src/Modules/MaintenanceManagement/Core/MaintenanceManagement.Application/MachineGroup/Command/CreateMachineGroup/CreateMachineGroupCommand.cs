using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.HttpResponse;
using MaintenanceManagement.Application.MachineGroup.Queries.GetMachineGroup;
using MediatR;

namespace MaintenanceManagement.Application.MachineGroup.Command.CreateMachineGroup
{
    public class CreateMachineGroupCommand : IRequest<int>
    {
        public string? GroupName { get; set; }
        public int Manufacturer { get; set; }
        public int UnitId { get; set; }
        public int DepartmentId { get; set; }
        public byte PowerSource { get; set; } 
    }
}