using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.HttpResponse;
using MediatR;

namespace MaintenanceManagement.Application.PreventiveSchedulers.Queries.GetDetailSchedulerByDate
{
    public class GetDetailSchedulerByDateQuery : IRequest<ApiResponseDTO<List<DetailSchedulerByDateDto>>>
    {
        public DateOnly SchedulerDate { get; set; }
        public int DepartmentId { get; set; }
    }
}