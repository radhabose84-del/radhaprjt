using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MaintenanceManagement.Application.PreventiveSchedulers.Queries.GetMachineDetailById
{
    public class MachineDetailItemsDto
    {
        public string OldItemId { get; set; }
        public string OldCategoryDescription { get; set; }
        public string OldGroupName { get; set; }
        public string? OldItemName { get; set; }
    }
}