using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.HttpResponse;
using MaintenanceManagement.Application.WorkCenter.Queries.GetWorkCenter;
using MediatR;

namespace MaintenanceManagement.Application.WorkCenter.Queries.GetWorkCenterById
{
    public class GetWorkCenterByIdQuery : IRequest<ApiResponseDTO<WorkCenterDto>>
    {
         public int Id { get; set; }
    }
}