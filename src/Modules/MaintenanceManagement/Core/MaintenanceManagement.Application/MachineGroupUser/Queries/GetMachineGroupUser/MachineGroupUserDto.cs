using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MaintenanceManagement.Application.MachineGroupUser.Queries.GetMachineGroupUser
{
    public class MachineGroupUserDto
    {
        public int Id { get; set; }
        public int MachineGroupId { get; set; }
        public string? GroupName { get; set; }
        public int DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public byte IsActive { get; set; }
        public byte IsDeleted { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
    }
}