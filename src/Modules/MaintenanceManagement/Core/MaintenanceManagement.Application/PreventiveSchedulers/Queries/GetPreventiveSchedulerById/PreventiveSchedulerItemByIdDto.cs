using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MaintenanceManagement.Application.PreventiveSchedulers.Queries.GetPreventiveSchedulerById
{
    public class PreventiveSchedulerItemByIdDto
    {
        public int Id { get; set; }
        public int PreventiveSchedulerHdrId { get; set; }
        public string OldItemId { get; set; }
        public int RequiredQty { get; set; }
        public string OldCategoryDescription { get; set; }
        public string OldGroupName { get; set; } 
        public string? OldItemName { get; set; }

    }
}