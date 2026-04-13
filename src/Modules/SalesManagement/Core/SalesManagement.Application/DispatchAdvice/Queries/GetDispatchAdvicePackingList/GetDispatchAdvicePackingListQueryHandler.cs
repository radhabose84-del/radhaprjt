using MediatR;
using SalesManagement.Application.Common.Interfaces.IDispatchAdvice;
using SalesManagement.Application.DispatchAdvice.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.DispatchAdvice.Queries.GetDispatchAdvicePackingList
{
    public class GetDispatchAdvicePackingListQueryHandler : IRequestHandler<GetDispatchAdvicePackingListQuery, DispatchAdvicePackingListDto?>
    {
        private readonly IDispatchAdviceQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetDispatchAdvicePackingListQueryHandler(
            IDispatchAdviceQueryRepository queryRepository,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<DispatchAdvicePackingListDto?> Handle(GetDispatchAdvicePackingListQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetPackingListAsync(request.DispatchAdviceId, cancellationToken);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetDispatchAdvicePackingListQuery",
                actionCode: "Get",
                actionName: request.DispatchAdviceId.ToString(),
                details: $"Packing list for Dispatch Advice {request.DispatchAdviceId} was fetched.",
                module: "DispatchAdvice");
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
