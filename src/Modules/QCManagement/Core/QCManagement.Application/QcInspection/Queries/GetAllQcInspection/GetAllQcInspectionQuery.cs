using Contracts.Common;
using MediatR;
using QCManagement.Application.QcInspection.Dto;

namespace QCManagement.Application.QcInspection.Queries.GetAllQcInspection
{
    public class GetAllQcInspectionQuery : IRequest<ApiResponseDTO<List<QcInspectionListDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
        public int? QcStatusId { get; set; }
        public DateTimeOffset? InspectionDateFrom { get; set; }
        public DateTimeOffset? InspectionDateTo { get; set; }
    }
}
