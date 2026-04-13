using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IDispatchAdvice;
using SalesManagement.Application.Common.Interfaces.IMiscMaster;
using SalesManagement.Application.DispatchAdvice.Dto;
using SalesManagement.Domain.Common;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.DispatchAdvice.Queries.GetDispatchAdvicePackRange
{
    public class GetDispatchAdvicePackRangeQueryHandler : IRequestHandler<GetDispatchAdvicePackRangeQuery, List<DispatchAdvicePackRangeDto>>
    {
        private readonly IDispatchAdviceQueryRepository _queryRepository;
        private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetDispatchAdvicePackRangeQueryHandler(
            IDispatchAdviceQueryRepository queryRepository,
            IMiscMasterQueryRepository miscMasterQueryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _miscMasterQueryRepository = miscMasterQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<List<DispatchAdvicePackRangeDto>> Handle(GetDispatchAdvicePackRangeQuery request, CancellationToken cancellationToken)
        {
            var packedStatus = await _miscMasterQueryRepository.GetMiscMasterByName(
                MiscEnumEntity.StockStatus, MiscEnumEntity.Packed);
            var statusId = packedStatus?.Id ?? 0;

            var result = await _queryRepository.GetPackRangeAsync(
                request.ItemId, request.LotId, request.PackTypeId, statusId, request.Range, request.OrderType);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetDispatchAdvicePackRangeQuery",
                actionCode: "Get",
                actionName: $"ItemId:{request.ItemId},LotId:{request.LotId},PackTypeId:{request.PackTypeId}",
                details: "Dispatch Advice pack range details were fetched.",
                module: "DispatchAdvice");
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
