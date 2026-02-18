using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using MaintenanceManagement.Application.WorkOrder.Command.CreateWorkOrder;
using MediatR;

namespace MaintenanceManagement.Application.MaintenanceRequest.Command.CreateExternalRequestWorkOrder
{
    public class CreateExternalRequestWorkOrderCommand  : IRequest<ApiResponseDTO<List<int>>>
    {
    public List<int>? Ids { get; set; } 
      

        
    }
}