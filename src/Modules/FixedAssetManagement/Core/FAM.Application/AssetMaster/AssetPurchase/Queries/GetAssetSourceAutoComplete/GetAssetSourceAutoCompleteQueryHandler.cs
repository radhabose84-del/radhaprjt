using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FAM.Application.Common.HttpResponse;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetPurchase;
using FAM.Domain.Events;
using MediatR;

namespace FAM.Application.AssetMaster.AssetPurchase.Queries.GetAssetSourceAutoComplete
{
    public class GetAssetSourceAutoCompleteQueryHandler :  IRequestHandler<GetAssetSourceAutoCompleteQuery,List<AssetSourceAutoCompleteDto>>
    {
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        
        private readonly IAssetPurchaseQueryRepository _iAssetPurchaseQueryRepository;

        public GetAssetSourceAutoCompleteQueryHandler(IMapper mapper, IMediator mediator, IAssetPurchaseQueryRepository iAssetPurchaseQueryRepository)
        {
            _mapper = mapper;
            _mediator = mediator;            
            _iAssetPurchaseQueryRepository = iAssetPurchaseQueryRepository;
        }

        public async Task<List<AssetSourceAutoCompleteDto>> Handle(GetAssetSourceAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _iAssetPurchaseQueryRepository.GetAssetSources(request.SearchPattern);
            var assetSources  = _mapper.Map<List<AssetSourceAutoCompleteDto>>(result);
             //Domain Event
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetAll",
                    actionCode: "GetAll",        
                    actionName: "AssetSource",
                    details: $"AssetSource details was fetched.",
                    module:"AssetSource"
                );
                await _mediator.Publish(domainEvent, cancellationToken);
            return assetSources;
        }
    }
}