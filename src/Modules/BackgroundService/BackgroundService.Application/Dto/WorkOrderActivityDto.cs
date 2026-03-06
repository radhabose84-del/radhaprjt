using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackgroundService.Application.Dto
{
    public class WorkOrderActivityDto
    {
         public int Id { get; set; }
        public int WorkOrderId { get; set; }
        public int ActivityId { get; set; }        
        public string? Description { get; set; }
    
    }
}