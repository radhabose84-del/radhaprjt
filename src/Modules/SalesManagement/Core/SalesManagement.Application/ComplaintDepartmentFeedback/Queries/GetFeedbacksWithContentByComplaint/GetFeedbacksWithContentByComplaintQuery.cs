using Contracts.Common;
using MediatR;
using SalesManagement.Application.ComplaintDepartmentFeedback.Dto;

namespace SalesManagement.Application.ComplaintDepartmentFeedback.Queries.GetFeedbacksWithContentByComplaint
{
    public sealed record GetFeedbacksWithContentByComplaintQuery(int ComplaintHeaderId)
        : IRequest<ApiResponseDTO<List<ComplaintFeedbackFullDto>>>;
}
