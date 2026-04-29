using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IComplaintDepartmentFeedback;
using SalesManagement.Application.ComplaintDepartmentFeedback.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.ComplaintDepartmentFeedback.Queries.GetAllFeedbacksForReviewer
{
    public class GetAllFeedbacksForReviewerQueryHandler
        : IRequestHandler<GetAllFeedbacksForReviewerQuery, ApiResponseDTO<List<FeedbackListDto>>>
    {
        private readonly IComplaintDepartmentFeedbackQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetAllFeedbacksForReviewerQueryHandler(
            IComplaintDepartmentFeedbackQueryRepository queryRepository,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<FeedbackListDto>>> Handle(
            GetAllFeedbacksForReviewerQuery request, CancellationToken cancellationToken)
        {
            // Pass null for responsiblePersonId so the repo's personFilter is skipped —
            // returns feedbacks across all departments / all users.
            var (data, totalCount) = await _queryRepository.GetAllAsync(
                request.PageNumber,
                request.PageSize,
                request.SearchTerm,
                request.StatusFilter,
                responsiblePersonId: null);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllForReviewer",
                actionCode: "GetAllFeedbacksForReviewerQuery",
                actionName: data.Count.ToString(),
                details: "Reviewer-scoped department feedback list was fetched.",
                module: "ComplaintDepartmentFeedback");
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<FeedbackListDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = data,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
