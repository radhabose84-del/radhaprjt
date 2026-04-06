using Contracts.Common;
using MediatR;
using SalesManagement.Application.ComplaintDepartmentFeedback.Dto;

namespace SalesManagement.Application.ComplaintDepartmentFeedback.Queries.GetAllFeedback
{
    public class GetAllComplaintDepartmentFeedbackQuery : IRequest<ApiResponseDTO<List<FeedbackListDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
        public string? StatusFilter { get; set; }
    }
}
