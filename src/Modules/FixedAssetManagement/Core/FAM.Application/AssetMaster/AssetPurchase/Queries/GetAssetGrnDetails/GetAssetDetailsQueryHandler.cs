using AutoMapper;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetPurchase;
using FAM.Domain.Entities.AssetPurchase;
using FAM.Domain.Events;
using MediatR;

namespace FAM.Application.AssetMaster.AssetPurchase.Queries.GetAssetGrnDetails
{
    public class GetAssetDetailsQueryHandler : IRequestHandler<GetAssetDetailsQuery, List<AssetGrnDetails>>
    {
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IAssetPurchaseQueryRepository _iAssetPurchaseQueryRepository;

        public GetAssetDetailsQueryHandler(IMapper mapper, IMediator mediator, IAssetPurchaseQueryRepository iAssetPurchaseQueryRepository)
        {
            _mapper = mapper;
            _mediator = mediator;
            _iAssetPurchaseQueryRepository = iAssetPurchaseQueryRepository;
        }

        public async Task<List<AssetGrnDetails>> Handle(GetAssetDetailsQuery request, CancellationToken cancellationToken)
        {
            var result = await _iAssetPurchaseQueryRepository.GetAssetGrnItemDetails(request.OldUnitId ?? string.Empty,request.AssetSourceId ,request.GrnNo,request.GrnSerialNo);
            var assetunits  = _mapper.Map<List<AssetGrnDetails>>(result);
             //Domain Event
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetGrnDetails",
                    actionCode: "GetAll",        
                    actionName: "GrnItemDetails",
                    details: $"GrnItemDetails details was fetched.",
                    module:"GrnItemDetails"
                );
                await _mediator.Publish(domainEvent, cancellationToken);
            return assetunits;
        }
    }
}