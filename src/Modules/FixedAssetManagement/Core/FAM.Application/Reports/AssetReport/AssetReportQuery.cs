
using FAM.Application.Common.HttpResponse;
using MediatR;

namespace FAM.Application.Reports.AssetReport
{
    public class AssetReportQuery : IRequest<ApiResponseDTO<List<AssetReportDto>>> 
    {
        public DateTimeOffset? FromDate { get; set; }
        public DateTimeOffset? ToDate { get; set; }
    }
}