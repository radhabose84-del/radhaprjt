using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FAM.Application.AssetSubCategories.Queries.GetAssetSubCategories;
using FAM.Application.Common.HttpResponse;
using FAM.Application.Common.Interfaces.IAssetSubCategories;
using FAM.Domain.Events;
using MediatR;

namespace FAM.Application.AssetSubCategories.Queries.GetAssetSubCategoriesAutoComplete
{
    public class GetAssetSubCategoriesAutoCompleteQueryHandler :  IRequestHandler<GetAssetSubCategoriesAutoCompleteQuery,List<AssetSubCategoriesAutoCompleteDto>>
    {
        
        private readonly IAssetSubCategoriesQueryRepository _iAssetSubCategoriesQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAssetSubCategoriesAutoCompleteQueryHandler(IAssetSubCategoriesQueryRepository iAssetSubCategoriesQueryRepository, IMapper mapper, IMediator mediator)
        {
            _iAssetSubCategoriesQueryRepository = iAssetSubCategoriesQueryRepository;            
            _mapper = mapper;
            _mediator = mediator;
        }
        
        public async Task<List<AssetSubCategoriesAutoCompleteDto>> Handle(GetAssetSubCategoriesAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _iAssetSubCategoriesQueryRepository.GetAssetSubCategories(request.SearchPattern);
            var assetsubcategories  = _mapper.Map<List<AssetSubCategoriesAutoCompleteDto>>(result);
             //Domain Event
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetAll",
                    actionCode: "",        
                    actionName: "",
                    details: $"AssetSubCategories details was fetched.",
                    module:"AssetSubCategories"
                );
                await _mediator.Publish(domainEvent, cancellationToken);
            return assetsubcategories;
        }

    }
}