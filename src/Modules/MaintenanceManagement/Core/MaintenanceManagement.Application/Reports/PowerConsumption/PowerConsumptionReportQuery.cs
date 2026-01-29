using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.HttpResponse;
using MediatR;

namespace MaintenanceManagement.Application.Reports.PowerConsumption
{
    public class PowerConsumptionReportQuery : IRequest<ApiResponseDTO<List<PowerReportDto>>>
    {
        public DateTimeOffset? FromDate { get; set; }
        public DateTimeOffset? ToDate { get; set; }
    }
}