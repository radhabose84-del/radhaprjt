using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Contracts.Dtos.Maintenance.Preventive
{
    public class RollbackItemsDto
    {
        public int Id { get; set; }
        public int PreventiveSchedulerHeaderId { get; set; }
        public int ItemId { get; set; }
        public int RequiredQty { get; set; }
        public string OldItemId { get; set; }
        public string OldCategoryDescription { get; set; }
        public string OldGroupName { get; set; }
        public string? OldItemName { get; set; }
        
    }
}