using Contracts.Common;
using MediatR;
using SalesManagement.Application.ComplaintDepartmentFeedback.Dto;

namespace SalesManagement.Application.ComplaintDepartmentFeedback.Queries.GetFeedbackByAssignment
{
    public class GetFeedbackByAssignmentIdQuery : IRequest<ApiResponseDTO<ComplaintDepartmentFeedbackDto>>
    {
        public int AssignmentId { get; set; }
    }
}
