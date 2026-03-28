using MediatR;
using SalesManagement.Application.Common.Interfaces.IComplaintResolution;
using SalesManagement.Application.ComplaintResolution.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.ComplaintResolution.Queries.GetResolutionByComplaintId
{
    public class GetResolutionByComplaintIdQueryHandler : IRequestHandler<GetResolutionByComplaintIdQuery, ComplaintResolutionDto?>
    {
        private readonly IComplaintResolutionQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetResolutionByComplaintIdQueryHandler(
            IComplaintResolutionQueryRepository queryRepository,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<ComplaintResolutionDto?> Handle(GetResolutionByComplaintIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByComplaintHeaderIdAsync(request.ComplaintHeaderId);

            if (result == null)
                return null;

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetByComplaintId",
                actionCode: "GetResolutionByComplaintIdQuery",
                actionName: request.ComplaintHeaderId.ToString(),
                details: $"Resolution for Complaint {request.ComplaintHeaderId} was fetched.",
                module: "ComplaintResolution");
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
