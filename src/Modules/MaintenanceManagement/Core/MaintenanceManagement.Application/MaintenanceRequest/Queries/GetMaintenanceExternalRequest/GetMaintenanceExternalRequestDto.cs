using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MaintenanceManagement.Application.MaintenanceRequest.Queries.GetMaintenanceExternalRequest
{
    public class GetMaintenanceExternalRequestDto
    {
        public int Id { get; set; }
       public int RequestTypeId  { get; set; }  
       public string? RequestType  { get; set; }   
       public int MaintenanceTypeId  { get; set; }    
       public string? MaintenanceType { get; set; }  
       public int MachineId { get; set; }
       public string? MachineName { get; set; }
       public int  CompanyId { get; set; }
       public   int  UnitId { get; set; }       
        public int ProductionDepartmentId { get; set; }
       public string? ProductionDepartmentName { get; set; }      
       public int MaintenanceDepartmentId { get; set; }
       public string? MaintenanceDepartmentName { get; set; }
       public int SourceId { get; set; }
       public int? VendorId  { get; set; }
       public string? VendorName  { get; set; } = null;
       public string? OldVendorId  { get; set; }
       public string?   OldVendorName  { get; set; }
       public int?  ServiceTypeId { get; set; }
       public string? ServiceType { get; set; }
       public int?  ServiceLocationId { get; set; }
       public string? ServiceLocation { get; set; }
       public int?  ModeOfDispatchId { get; set; }
       public string? ModeOfDispatch { get; set; }
       public string? ExpectedDispatchDate { get; set; }
       public int?  SparesTypeId { get; set; }
       public string? SparesType { get; set; }     
        public decimal EstimatedServiceCost { get; set; }
        public decimal EstimatedSpareCost { get; set; }
       public string? Remarks { get; set; }
       public int RequestStatusId { get; set; }
       public string? RequestStatus { get; set; }
       public string? CreatedByName { get; set; }
       public DateTimeOffset CreatedDate { get; set; }
       public int  CreatedBy { get; set; }
       public string?  CreatedIP { get; set; }
       public string? ModifiedByName { get; set; }
       public DateTimeOffset? ModifiedDate { get; set;}
       public int? ModifiedBy { get; set;}
       public string? ModifiedIP { get; set;}
    }
}