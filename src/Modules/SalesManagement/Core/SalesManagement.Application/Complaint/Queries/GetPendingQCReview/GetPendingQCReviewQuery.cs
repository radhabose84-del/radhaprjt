using MediatR;
using SalesManagement.Application.Complaint.Dto;

namespace SalesManagement.Application.Complaint.Queries.GetPendingQCReview
{
    public class GetPendingQCReviewQuery
        : IRequest<(List<PendingQCReviewListDto> Items, int TotalCount)>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
    }
}
