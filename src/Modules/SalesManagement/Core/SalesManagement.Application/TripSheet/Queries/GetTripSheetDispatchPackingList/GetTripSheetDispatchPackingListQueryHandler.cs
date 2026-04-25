using MediatR;
using SalesManagement.Application.Common.Interfaces.IDispatchAdvice;
using SalesManagement.Application.DispatchAdvice.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.TripSheet.Queries.GetTripSheetDispatchPackingList
{
    public class GetTripSheetDispatchPackingListQueryHandler : IRequestHandler<GetTripSheetDispatchPackingListQuery, List<DispatchAdvicePackingListDto>>
    {
        private readonly IDispatchAdviceQueryRepository _dispatchAdviceQueryRepository;
        private readonly IMediator _mediator;

        public GetTripSheetDispatchPackingListQueryHandler(
            IDispatchAdviceQueryRepository dispatchAdviceQueryRepository,
            IMediator mediator)
        {
            _dispatchAdviceQueryRepository = dispatchAdviceQueryRepository;
            _mediator = mediator;
        }

        public async Task<List<DispatchAdvicePackingListDto>> Handle(GetTripSheetDispatchPackingListQuery request, CancellationToken cancellationToken)
        {
            var result = await _dispatchAdviceQueryRepository.GetPackingListByTripSheetAsync(request.TripSheetHeaderId, cancellationToken);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetTripSheetDispatchPackingList",
                actionCode: "GetTripSheetDispatchPackingListQuery",
                actionName: result.Count.ToString(),
                details: $"Dispatch packing lists for TripSheet {request.TripSheetHeaderId} were fetched.",
                module: "TripSheet"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
