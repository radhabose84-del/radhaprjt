using MediatR;
using SalesManagement.Application.ComplaintQCReview.Dto;

namespace SalesManagement.Application.ComplaintQCReview.Queries.GetQCReviewByComplaintId
{
    public class GetQCReviewByComplaintIdQuery : IRequest<ComplaintQCReviewDto?>
    {
        public int ComplaintHeaderId { get; set; }
    }
}
