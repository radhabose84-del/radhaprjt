using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IComplaintDepartmentFeedback;
using SalesManagement.Application.ComplaintDepartmentFeedback.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.ComplaintDepartmentFeedback.Queries.GetFeedbacksByComplaint
{
    public class GetFeedbacksByComplaintIdQueryHandler : IRequestHandler<GetFeedbacksByComplaintIdQuery, ApiResponseDTO<List<FeedbackListDto>>>
    {
        private readonly IComplaintDepartmentFeedbackQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetFeedbacksByComplaintIdQueryHandler(
            IComplaintDepartmentFeedbackQueryRepository queryRepository,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<FeedbackListDto>>> Handle(GetFeedbacksByComplaintIdQuery request, CancellationToken cancellationToken)
        {
            var data = await _queryRepository.GetByComplaintIdAsync(request.ComplaintHeaderId);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetByComplaint",
                actionCode: "GetFeedbacksByComplaintIdQuery",
                actionName: request.ComplaintHeaderId.ToString(),
                details: $"All feedbacks for complaint {request.ComplaintHeaderId} were fetched.",
                module: "ComplaintDepartmentFeedback");
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<FeedbackListDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = data,
                TotalCount = data.Count
            };
        }
    }
}
