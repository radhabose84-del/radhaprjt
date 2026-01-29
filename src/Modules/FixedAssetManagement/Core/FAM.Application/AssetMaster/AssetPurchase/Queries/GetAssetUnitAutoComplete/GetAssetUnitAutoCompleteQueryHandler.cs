using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FAM.Application.AssetCategories.Queries.GetAssetCategoriesAutoComplete;
using FAM.Application.AssetMaster.AssetPurchase.Queries.GetAssetSourceAutoComplete;
using FAM.Application.Common.HttpResponse;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetPurchase;
using FAM.Domain.Events;
using MediatR;

namespace FAM.Application.AssetMaster.AssetPurchase.Queries
{
    public class GetAssetUnitAutoCompleteQueryHandler  :  IRequestHandler<GetAssetUnitAutoCompleteQuery,List<AssetUnitAutoCompleteDto>>
    {
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
         private readonly IAssetPurchaseQueryRepository _iAssetPurchaseQueryRepository;


        public GetAssetUnitAutoCompleteQueryHandler(IMapper mapper, IMediator mediator, IAssetPurchaseQueryRepository iAssetPurchaseQueryRepository)
        {
            _mapper = mapper;
            _mediator = mediator;
            _iAssetPurchaseQueryRepository=iAssetPurchaseQueryRepository;
        }


        public async Task<List<AssetUnitAutoCompleteDto>> Handle(GetAssetUnitAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _iAssetPurchaseQueryRepository.GetAssetUnit(request.Username);
            var assetunits  = _mapper.Map<List<AssetUnitAutoCompleteDto>>(result);
             //Domain Event
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetAll",
                    actionCode: "GetAll",        
                    actionName: "Assetunit",
                    details: $"Assetunit details was fetched.",
                    module:"Assetunit"
                );
                await _mediator.Publish(domainEvent, cancellationToken);
            return assetunits;
        }
    }
}