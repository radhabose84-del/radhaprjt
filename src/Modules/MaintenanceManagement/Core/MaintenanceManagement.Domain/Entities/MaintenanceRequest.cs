using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Domain.Common;
using MaintenanceManagement.Domain.Entities.WorkOrderMaster;

namespace MaintenanceManagement.Domain.Entities
{
    public class MaintenanceRequest  : BaseEntity
    {
       public int RequestTypeId  { get; set; } 
       public MiscMaster? MiscRequestType { get; set; }
       public int MaintenanceTypeId  { get; set; }
       public MiscMaster? MiscMaintenanceType { get; set; }
       public int MachineId { get; set; }        
       public MachineMaster Machine { get; set; } = null!;
       public int  CompanyId { get; set; }
       public int  UnitId { get; set; }
       public int MaintenanceDepartmentId { get; set; }
       public int ProductionDepartmentId { get; set; }

       public int SourceId { get; set; }
       public int? VendorId  { get; set; }
       public string? VendorName { get; set; }
       public string? OldVendorId  { get; set; }
      public string? OldVendorName { get; set; }
       public int? ServiceTypeId { get; set;}
       public MiscMaster? ServiceType { get; set; } 
       public int? ServiceLocationId { get; set;}
       public MiscMaster? ServiceLocation { get; set; } 
       public int? ModeOfDispatchId { get; set;}
       public MiscMaster? ModeOfDispatchType { get; set; }        
       public DateTimeOffset? ExpectedDispatchDate { get; set; }
       public int? SparesTypeId { get; set; }
       public MiscMaster? SpareType { get; set; }         
       public decimal? EstimatedServiceCost { get; set;} = null!;
       public decimal? EstimatedSpareCost { get; set;}
    
       public string? Remarks { get; set; } 
       public int?  RequestStatusId { get; set; }  
       public MiscMaster? RequestStatus { get; set; }    
       public ICollection<WorkOrder>? WorkOrdersRequest  {get; set;}        

    }
}
