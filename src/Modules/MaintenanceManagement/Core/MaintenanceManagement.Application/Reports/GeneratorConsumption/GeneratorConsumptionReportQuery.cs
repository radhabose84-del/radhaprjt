using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.HttpResponse;
using MediatR;

namespace MaintenanceManagement.Application.Reports.GeneratorConsumption
{
    public class GeneratorConsumptionReportQuery : IRequest<ApiResponseDTO<List<GeneratorReportDto>>>
    {
        public DateTimeOffset? FromDate { get; set; }
        public DateTimeOffset? ToDate { get; set; }
    }
}