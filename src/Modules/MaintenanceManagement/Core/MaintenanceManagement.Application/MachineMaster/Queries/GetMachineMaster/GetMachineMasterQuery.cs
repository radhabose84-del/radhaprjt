using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.HttpResponse;
using MediatR;

namespace MaintenanceManagement.Application.MachineMaster.Queries.GetMachineMaster
{
    public class GetMachineMasterQuery : IRequest<ApiResponseDTO<List<MachineMasterDto>>>
    {
        public string? SearchTerm { get; set; }
    }
}