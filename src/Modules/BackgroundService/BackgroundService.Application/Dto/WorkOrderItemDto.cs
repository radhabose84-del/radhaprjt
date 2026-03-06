using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackgroundService.Application.Dto
{
    public class WorkOrderItemDto
    {
        public int Id { get; set; }
        public int? WorkOrderId { get; set; }  
        public int? StoreTypeId { get; set; }      
        public string? ItemCode { get; set; }        
        public string? OldItemCode { get; set; }         
        public string? ItemName { get; set; }          
        public int AvailableQty { get; set; }
        public int UsedQty { get; set; }
        public int? ScarpQty { get; set; }
        public int? ToSubStoreQty { get; set; }
        public string? Image { get; set; }
    }
}