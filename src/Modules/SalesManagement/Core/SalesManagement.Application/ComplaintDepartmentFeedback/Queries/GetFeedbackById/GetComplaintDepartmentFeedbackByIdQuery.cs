using Contracts.Common;
using MediatR;
using SalesManagement.Application.ComplaintDepartmentFeedback.Dto;

namespace SalesManagement.Application.ComplaintDepartmentFeedback.Queries.GetFeedbackById
{
    public class GetComplaintDepartmentFeedbackByIdQuery : IRequest<ApiResponseDTO<ComplaintDepartmentFeedbackDto>>
    {
        public int Id { get; set; }
    }
}
