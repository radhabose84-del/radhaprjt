using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.HttpResponse;
using MediatR;

namespace MaintenanceManagement.Application.PreventiveSchedulers.Queries.GetMachineDetailById
{
    public class GetMachineDetailByIdQuery : IRequest<ApiResponseDTO<PreventiveSchedulerDto>>
    {
        public int Id { get; set; }
    }
}