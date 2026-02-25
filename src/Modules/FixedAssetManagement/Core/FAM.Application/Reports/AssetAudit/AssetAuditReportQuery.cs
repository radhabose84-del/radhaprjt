using Contracts.Common;
using MediatR;

namespace FAM.Application.Reports.AssetAudit
{
    public class AssetAuditReportQuery : IRequest<ApiResponseDTO<List<AssetAuditReportDto>>>
    {
        public int AuditCycle { get; set; }
    }
}