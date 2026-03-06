using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IDispatchAdvice;
using SalesManagement.Application.Common.Interfaces.IMiscMaster;
using SalesManagement.Application.DispatchAdvice.Dto;
using SalesManagement.Domain.Common;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.DispatchAdvice.Queries.GetDispatchAdviceStock
{
    public class GetDispatchAdviceStockQueryHandler : IRequestHandler<GetDispatchAdviceStockQuery, DispatchAdviceStockDto?>
    {
        private readonly IDispatchAdviceQueryRepository _queryRepository;
        private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetDispatchAdviceStockQueryHandler(
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

        public async Task<DispatchAdviceStockDto?> Handle(GetDispatchAdviceStockQuery request, CancellationToken cancellationToken)
        {
            var packedStatus = await _miscMasterQueryRepository.GetMiscMasterByName(
                MiscEnumEntity.StockStatus, MiscEnumEntity.Packed);
            var statusId = packedStatus?.Id ?? 0;

            var result = await _queryRepository.GetStockAsync(request.ItemId, request.LotId, statusId);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetDispatchAdviceStockQuery",
                actionCode: "Get",
                actionName: $"ItemId:{request.ItemId},LotId:{request.LotId}",
                details: "Dispatch Advice stock details were fetched.",
                module: "DispatchAdvice");
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
