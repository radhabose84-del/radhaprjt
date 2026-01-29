using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.HttpResponse;
using MaintenanceManagement.Application.MaintenanceRequest.Queries.GetMaintenanceRequest;
using MediatR;

namespace MaintenanceManagement.Application.MaintenanceRequest.Queries.GetMaintenanceRequestById
{
    public class GetMaintenanceRequestByIdQuery  :  IRequest<ApiResponseDTO<GetMaintenanceRequestDto>>
    {
        public int Id { get; set; }
    }
}