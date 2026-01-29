using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static MaintenanceManagement.Domain.Common.BaseEntity;

namespace MaintenanceManagement.Application.MachineGroup.Queries.GetMachineGroupById
{
    public class GetMachineGroupByIdDto
    {
        public int Id { get; set; }
        public string? GroupName { get; set; }
        public int Manufacturer { get; set; }
        public int DepartmentId { get; set; }
        public Status IsActive { get; set; }
        public IsDelete IsDeleted { get; set; }
        public byte PowerSource { get; set; }
    }
}