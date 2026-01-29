using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.HttpResponse;
using MediatR;

namespace MaintenanceManagement.Application.PreventiveSchedulers.Queries.GetUnMappedMachine
{
    public class GetUnMappedMachineQuery : IRequest<ApiResponseDTO<List<UnMappedMachineDto>>>
    {
        public int Id { get; set; }
    }
}