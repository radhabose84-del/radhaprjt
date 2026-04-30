using MediatR;
using SalesManagement.Application.Common.Interfaces.IDispatchAdvice;
using SalesManagement.Application.DispatchAdvice.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.DispatchAdvice.Queries.GetDispatchAdvicePackingListByIds
{
    public class GetDispatchAdvicePackingListByIdsQueryHandler : IRequestHandler<GetDispatchAdvicePackingListByIdsQuery, List<DispatchAdvicePackingListDto>>
    {
        private readonly IDispatchAdviceQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetDispatchAdvicePackingListByIdsQueryHandler(
            IDispatchAdviceQueryRepository queryRepository,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<List<DispatchAdvicePackingListDto>> Handle(GetDispatchAdvicePackingListByIdsQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetPackingListByIdsAsync(request.DispatchAdviceIds, cancellationToken);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetDispatchAdvicePackingListByIds",
                actionCode: "GetDispatchAdvicePackingListByIdsQuery",
                actionName: result.Count.ToString(),
                details: $"Packing lists for {request.DispatchAdviceIds.Count} Dispatch Advice(s) were fetched.",
                module: "DispatchAdvice");
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
