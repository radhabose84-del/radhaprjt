using AutoMapper;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetPurchase;
using FAM.Domain.Events;
using MediatR;

namespace FAM.Application.AssetMaster.AssetPurchase.Queries.GetAssetGRNItem
{
    public class GetAssetGrnItemQueryHandler : IRequestHandler<GetAssetGrnItemQuery, List<AssetGrnItemDto>>
    {
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IAssetPurchaseQueryRepository _iAssetPurchaseQueryRepository;

        public GetAssetGrnItemQueryHandler(IMapper mapper, IMediator mediator, IAssetPurchaseQueryRepository iAssetPurchaseQueryRepository)
        {
            _mapper = mapper;
            _mediator = mediator;
            _iAssetPurchaseQueryRepository = iAssetPurchaseQueryRepository;
        }

        public async Task<List<AssetGrnItemDto>> Handle(GetAssetGrnItemQuery request, CancellationToken cancellationToken)
        {
            var result = await _iAssetPurchaseQueryRepository.GetAssetGrnItem(request.OldUnitId,request.AssetSourceId ,request.GrnNo);
            var assetunits  = _mapper.Map<List<AssetGrnItemDto>>(result);
             //Domain Event
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetGrnItem",
                    actionCode: "GetAll",        
                    actionName: "GrnItem",
                    details: $"GrnItem details was fetched.",
                    module:"GrnItem"
                );
                await _mediator.Publish(domainEvent, cancellationToken);
            return  assetunits;
        }
    }
}