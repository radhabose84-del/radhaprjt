using MediatR;
using SalesManagement.Application.ComplaintQCReview.Dto;

namespace SalesManagement.Application.ComplaintQCReview.Queries.GetQCReviewById
{
    public class GetQCReviewByIdQuery : IRequest<ComplaintQCReviewDto?>
    {
        public int Id { get; set; }
    }
}
