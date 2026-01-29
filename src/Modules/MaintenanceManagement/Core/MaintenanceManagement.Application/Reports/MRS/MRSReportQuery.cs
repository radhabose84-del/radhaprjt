using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.HttpResponse;
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