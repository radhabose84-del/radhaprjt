using Contracts.Common;
using MediatR;
using SalesManagement.Application.ComplaintDepartmentFeedback.Dto;

namespace SalesManagement.Application.ComplaintDepartmentFeedback.Queries.GetFeedbacksByComplaint
{
    public class GetFeedbacksByComplaintIdQuery : IRequest<ApiResponseDTO<List<FeedbackListDto>>>
    {
        public int ComplaintHeaderId { get; set; }
    }
}
