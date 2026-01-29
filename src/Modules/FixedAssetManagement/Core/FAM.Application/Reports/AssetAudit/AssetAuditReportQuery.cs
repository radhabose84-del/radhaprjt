using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FAM.Application.Common.HttpResponse;
using MediatR;

namespace FAM.Application.Reports.AssetAudit
{
    public class AssetAuditReportQuery : IRequest<ApiResponseDTO<List<AssetAuditReportDto>>>
    {
        public int AuditCycle { get; set; }
    }
}