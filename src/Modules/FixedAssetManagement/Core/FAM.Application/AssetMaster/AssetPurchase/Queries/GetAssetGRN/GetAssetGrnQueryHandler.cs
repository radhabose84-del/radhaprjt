using AutoMapper;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetPurchase;
using FAM.Domain.Events;
using MediatR;

namespace FAM.Application.AssetMaster.AssetPurchase.Queries.GetAssetGRN
{
    public class GetAssetGrnQueryHandler :  IRequestHandler<GetAssetGrnQuery,List<GetAssetGrnDto>>
    {
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IAssetPurchaseQueryRepository _iAssetPurchaseQueryRepository;

        public GetAssetGrnQueryHandler(IMapper mapper, IMediator mediator, IAssetPurchaseQueryRepository iAssetPurchaseQueryRepository)
        {
            _mapper = mapper;
            _mediator = mediator;
            _iAssetPurchaseQueryRepository = iAssetPurchaseQueryRepository;
        }

        public async Task<List<GetAssetGrnDto>> Handle(GetAssetGrnQuery request, CancellationToken cancellationToken)
        {
             var result = await _iAssetPurchaseQueryRepository.GetAssetGrnNo(request.OldUnitId ?? string.Empty,request.AssetSourceId,request.SearchGrnNo);
            var assetunits  = _mapper.Map<List<GetAssetGrnDto>>(result);
             //Domain Event
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetAll",
                    actionCode: "GetAll",        
                    actionName: "GRNNO",
                    details: $"GRN details was fetched.",
                    module:"GRNNO"
                );
                await _mediator.Publish(domainEvent, cancellationToken);
            return assetunits;
        }
    }
}