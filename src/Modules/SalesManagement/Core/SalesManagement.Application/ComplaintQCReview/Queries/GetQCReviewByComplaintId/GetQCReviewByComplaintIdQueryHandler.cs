using MediatR;
using SalesManagement.Application.Common.Interfaces.IComplaintQCReview;
using SalesManagement.Application.ComplaintQCReview.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.ComplaintQCReview.Queries.GetQCReviewByComplaintId
{
    public class GetQCReviewByComplaintIdQueryHandler : IRequestHandler<GetQCReviewByComplaintIdQuery, ComplaintQCReviewDto?>
    {
        private readonly IComplaintQCReviewQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetQCReviewByComplaintIdQueryHandler(
            IComplaintQCReviewQueryRepository queryRepository,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<ComplaintQCReviewDto?> Handle(GetQCReviewByComplaintIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByComplaintIdAsync(request.ComplaintHeaderId);

            if (result == null)
                return null;

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetByComplaintId",
                actionCode: "GetQCReviewByComplaintIdQuery",
                actionName: request.ComplaintHeaderId.ToString(),
                details: $"QC Review for Complaint {request.ComplaintHeaderId} was fetched.",
                module: "ComplaintQCReview");
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
