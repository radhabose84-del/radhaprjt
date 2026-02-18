using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using MediatR;

namespace MaintenanceManagement.Application.MaintenanceRequest.Command.UpdateMaintenanceRequestCommand
{
    public class UpdateMaintenanceRequestCommand   :  IRequest<ApiResponseDTO<bool>>
    { 
       public int Id { get; set; }
       public int RequestTypeId  { get; set; }       
       public int MaintenanceTypeId  { get; set; }     
       public int MachineId { get; set; } 
        public int ProductionDepartmentId { get; set; }
       public int MaintenanceDepartmentId { get; set; }
       public int? SourceId { get; set; }
       public int? VendorId  { get; set; }
       public string? VendorName { get; set; }
       public string? OldVendorId  { get; set; }
       public string? OldVendorName { get; set; }
       public int?  ServiceTypeId { get; set; }
       public int?  ServiceLocationId { get; set; }
       public int? ModeOfDispatchId { get; set; }
       public DateTimeOffset? ExpectedDispatchDate { get; set; }
       public int? SparesTypeId { get; set; }
       public decimal? EstimatedServiceCost { get; set; }
       public decimal? EstimatedSpareCost { get; set; }
       public string? Remarks { get; set; }
       public int RequestStatusId { get; set; }
        
    }
}