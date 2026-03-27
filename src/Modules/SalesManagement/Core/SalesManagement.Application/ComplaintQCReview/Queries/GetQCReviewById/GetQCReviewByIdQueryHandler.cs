using MediatR;
using SalesManagement.Application.Common.Interfaces.IComplaintQCReview;
using SalesManagement.Application.ComplaintQCReview.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.ComplaintQCReview.Queries.GetQCReviewById
{
    public class GetQCReviewByIdQueryHandler : IRequestHandler<GetQCReviewByIdQuery, ComplaintQCReviewDto?>
    {
        private readonly IComplaintQCReviewQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetQCReviewByIdQueryHandler(
            IComplaintQCReviewQueryRepository queryRepository,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<ComplaintQCReviewDto?> Handle(GetQCReviewByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.Id);

            if (result == null)
                return null;

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetQCReviewByIdQuery",
                actionName: result.Id.ToString(),
                details: $"QC Review details {result.Id} was fetched.",
                module: "ComplaintQCReview");
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
