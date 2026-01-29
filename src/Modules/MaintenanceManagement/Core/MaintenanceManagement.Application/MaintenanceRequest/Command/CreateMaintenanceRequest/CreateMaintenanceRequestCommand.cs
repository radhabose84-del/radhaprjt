using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.HttpResponse;
using MaintenanceManagement.Application.MaintenanceRequest.Queries.GetMaintenanceRequest;
using MediatR;

namespace MaintenanceManagement.Application.MaintenanceRequest.Command.CreateMaintenanceRequest
{
    public class CreateMaintenanceRequestCommand  : IRequest<ApiResponseDTO<int>>
    {
      
       public int RequestTypeId  { get; set; }       
       public int MaintenanceTypeId  { get; set; }     
       public int MachineId { get; set; }               
       public int ProductionDepartmentId { get; set; }
       public int MaintenanceDepartmentId { get; set; }
       public int? SourceId { get; set; }
       public int? VendorId  { get; set; }
       public string? VendorName { get; set; } = null;
       public string? OldVendorId  { get; set; }
       public string? OldVendorName { get; set; } = null;
       public int?  ServiceTypeId { get; set; }
       public int?  ServiceLocationId { get; set; }
       public int? ModeOfDispatchId { get; set; }
       public DateTimeOffset? ExpectedDispatchDate { get; set; }
       public int? SparesTypeId { get; set; }
       public decimal? EstimatedServiceCost { get; set; }
       public decimal? EstimatedSpareCost { get; set; }
       public string? Remarks { get; set; }

        
    }
}