using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IComplaintDepartmentFeedback;
using SalesManagement.Application.ComplaintDepartmentFeedback.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.ComplaintDepartmentFeedback.Queries.GetFeedbackByAssignment
{
    public class GetFeedbackByAssignmentIdQueryHandler : IRequestHandler<GetFeedbackByAssignmentIdQuery, ApiResponseDTO<ComplaintDepartmentFeedbackDto>>
    {
        private readonly IComplaintDepartmentFeedbackQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetFeedbackByAssignmentIdQueryHandler(
            IComplaintDepartmentFeedbackQueryRepository queryRepository,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<ComplaintDepartmentFeedbackDto>> Handle(GetFeedbackByAssignmentIdQuery request, CancellationToken cancellationToken)
        {
            var data = await _queryRepository.GetByAssignmentIdAsync(request.AssignmentId);

            if (data == null)
            {
                return new ApiResponseDTO<ComplaintDepartmentFeedbackDto>
                {
                    IsSuccess = false,
                    Message = "No feedback found for this assignment.",
                    Data = null
                };
            }

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetByAssignment",
                actionCode: "GetFeedbackByAssignmentIdQuery",
                actionName: request.AssignmentId.ToString(),
                details: $"Feedback for assignment {request.AssignmentId} was fetched.",
                module: "ComplaintDepartmentFeedback");
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<ComplaintDepartmentFeedbackDto>
            {
                IsSuccess = true,
                Message = "Success",
                Data = data
            };
        }
    }
}
