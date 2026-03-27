using Contracts.Common;
using MediatR;
using SalesManagement.Application.ComplaintQCReview.Dto;

namespace SalesManagement.Application.ComplaintQCReview.Queries.GetAllQCReview
{
    public class GetAllQCReviewQuery : IRequest<ApiResponseDTO<List<QCReviewListDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
        public string? StatusFilter { get; set; }
    }
}
