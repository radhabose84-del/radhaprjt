using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FAM.Application.Common.HttpResponse;
using FAM.Application.Common.Interfaces.IAssetSubCategories;
using FAM.Domain.Events;
using MediatR;

namespace FAM.Application.AssetSubCategories.Queries.GetAssetSubCategories
{
    public class GetAssetSubCategoriesQueryHandler : IRequestHandler<GetAssetSubCategoriesQuery, ApiResponseDTO<List<AssetSubCategoriesDto>>>
    {
        private readonly IAssetSubCategoriesQueryRepository _iAssetSubCategoriesQueryRepository;        
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAssetSubCategoriesQueryHandler(IAssetSubCategoriesQueryRepository iAssetSubCategoriesQueryRepository, IMapper mapper, IMediator mediator)
        {
            _iAssetSubCategoriesQueryRepository = iAssetSubCategoriesQueryRepository;            
            _mapper = mapper;
            _mediator = mediator;
        }
          public async Task<ApiResponseDTO<List<AssetSubCategoriesDto>>> Handle(GetAssetSubCategoriesQuery request, CancellationToken cancellationToken)
        {
            var (assetsubcategories, totalCount) = await _iAssetSubCategoriesQueryRepository.GetAllAssetSubCategoriesAsync(request.PageNumber, request.PageSize, request.SearchTerm);
            var assetsubCategorieslist = _mapper.Map<List<AssetSubCategoriesDto>>(assetsubcategories);

             //Domain Event
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetAssetSubCategories",
                    actionCode: "",        
                    actionName: "",
                    details: $"AssetSubCategories details was fetched.",
                    module:"AssetSubCategories"
                );
                await _mediator.Publish(domainEvent, cancellationToken);
            return new ApiResponseDTO<List<AssetSubCategoriesDto>> 
            { 
                IsSuccess = true, 
                Message = "Success", 
                Data = assetsubCategorieslist ,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
                };
        }
    }
}