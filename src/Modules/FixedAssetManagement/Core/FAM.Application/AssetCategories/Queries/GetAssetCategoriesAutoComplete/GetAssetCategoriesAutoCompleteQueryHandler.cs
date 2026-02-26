using AutoMapper;
using FAM.Application.AssetCategories.Queries.GetAssetCategories;
using FAM.Application.Common.Interfaces.IAssetCategories;
using FAM.Domain.Events;
using MediatR;

namespace FAM.Application.AssetCategories.Queries.GetAssetCategoriesAutoComplete
{
    public class GetAssetCategoriesAutoCompleteQueryHandler :  IRequestHandler<GetAssetCategoriesAutoCompleteQuery,List<AssetCategoriesAutoCompleteDto>>
    {
        private readonly IAssetCategoriesQueryRepository _iAssetCategoriesQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAssetCategoriesAutoCompleteQueryHandler(IAssetCategoriesQueryRepository iAssetCategoriesQueryRepository, IMapper mapper, IMediator mediator)
        {
            _iAssetCategoriesQueryRepository = iAssetCategoriesQueryRepository;            
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<List<AssetCategoriesAutoCompleteDto>> Handle(GetAssetCategoriesAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _iAssetCategoriesQueryRepository.GetAssetCategories(request.SearchPattern);
            var assetcategories  = _mapper.Map<List<AssetCategoriesAutoCompleteDto>>(result);
             //Domain Event
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetAll",
                    actionCode: "",        
                    actionName: "",
                    details: $"AssetCategories details was fetched.",
                    module:"AssetCategories"
                );
                await _mediator.Publish(domainEvent, cancellationToken);
            return  assetcategories;
        }
    }
}