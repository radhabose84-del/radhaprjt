using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PurchaseManagement.Domain.Common;

namespace PurchaseManagement.Domain.Entities
{
    public class ServiceMaster : BaseEntity
    {
        public string? ServiceCode { get;  set; }
        public string ServiceDescription { get;  set; } = default!;
        public int SacId{ get;  set; } = default!;
        public int UomId { get;  set; }            
        public int  ServiceCategoryId { get;  set; }
        
    }
}