using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IComplaintDepartmentFeedback;
using SalesManagement.Application.ComplaintDepartmentFeedback.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.ComplaintDepartmentFeedback.Queries.GetFeedbacksWithContentByComplaint
{
    public class GetFeedbacksWithContentByComplaintQueryHandler
        : IRequestHandler<GetFeedbacksWithContentByComplaintQuery, ApiResponseDTO<List<ComplaintFeedbackFullDto>>>
    {
        private readonly IComplaintDepartmentFeedbackQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetFeedbacksWithContentByComplaintQueryHandler(
            IComplaintDepartmentFeedbackQueryRepository queryRepository,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<ComplaintFeedbackFullDto>>> Handle(
            GetFeedbacksWithContentByComplaintQuery request, CancellationToken cancellationToken)
        {
            var data = await _queryRepository.GetByComplaintIdWithContentAsync(request.ComplaintHeaderId);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetWithContentByComplaint",
                actionCode: "GetFeedbacksWithContentByComplaintQuery",
                actionName: request.ComplaintHeaderId.ToString(),
                details: $"Full RCA content for complaint {request.ComplaintHeaderId} fetched ({data.Count} assignments).",
                module: "ComplaintDepartmentFeedback");
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<ComplaintFeedbackFullDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = data,
                TotalCount = data.Count
            };
        }
    }
}
