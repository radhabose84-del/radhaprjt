using Contracts.Common;
using MediatR;

namespace MaintenanceManagement.Application.Reports.MRS
{
    public class MRSReportQuery : IRequest<ApiResponseDTO<List<MRSReportDto>>>
    {
        public DateTimeOffset? FromDate { get; set; }
        public DateTimeOffset? ToDate { get; set; }
        public string? OldUnitCode { get; set; }
    }
}