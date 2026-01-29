using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.HttpResponse;
using MediatR;

namespace MaintenanceManagement.Application.PreventiveSchedulers.Queries.GetPreventiveSchedulerById
{
    public class GetPreventiveSchedulerByIdQuery : IRequest<ApiResponseDTO<PreventiveSchedulerHdrByIdDto>>
    {
        public int Id { get; set; }
    }
}