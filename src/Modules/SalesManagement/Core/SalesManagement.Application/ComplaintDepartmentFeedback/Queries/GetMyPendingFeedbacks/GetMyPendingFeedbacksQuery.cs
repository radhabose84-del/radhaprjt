using Contracts.Common;
using MediatR;
using SalesManagement.Application.ComplaintDepartmentFeedback.Dto;

namespace SalesManagement.Application.ComplaintDepartmentFeedback.Queries.GetMyPendingFeedbacks
{
    public class GetMyPendingFeedbacksQuery : IRequest<ApiResponseDTO<List<MyPendingFeedbackDto>>>
    {
    }
}
