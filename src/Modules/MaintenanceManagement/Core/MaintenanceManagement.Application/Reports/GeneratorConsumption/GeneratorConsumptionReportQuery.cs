using Contracts.Common;
using MediatR;

namespace MaintenanceManagement.Application.Reports.GeneratorConsumption
{
    public class GeneratorConsumptionReportQuery : IRequest<ApiResponseDTO<List<GeneratorReportDto>>>
    {
        public DateTimeOffset? FromDate { get; set; }
        public DateTimeOffset? ToDate { get; set; }
    }
}