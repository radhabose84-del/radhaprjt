using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Common;
using FAM.Application.Common.Interfaces.IAssetCategories;
using FAM.Domain.Events;
using MediatR;

namespace FAM.Application.AssetCategories.Queries.GetAssetCategories
{
    public class GetAssetCategoriesQueryHandler : IRequestHandler<GetAssetCategoriesQuery, ApiResponseDTO<List<AssetCategoriesDto>>>
    {
        private readonly IAssetCategoriesQueryRepository _iAssetCategoriesQueryRepository;        
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAssetCategoriesQueryHandler(IAssetCategoriesQueryRepository iAssetCategoriesQueryRepository, IMapper mapper, IMediator mediator)
        {
            _iAssetCategoriesQueryRepository = iAssetCategoriesQueryRepository;            
            _mapper = mapper;
            _mediator = mediator;
        }
        public async Task<ApiResponseDTO<List<AssetCategoriesDto>>> Handle(GetAssetCategoriesQuery request, CancellationToken cancellationToken)
        {
            var (assetcategories, totalCount) = await _iAssetCategoriesQueryRepository.GetAllAssetCategoriesAsync(request.PageNumber, request.PageSize, request.SearchTerm);
            var assetCategorieslist = _mapper.Map<List<AssetCategoriesDto>>(assetcategories);

             //Domain Event
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetAssetCategories",
                    actionCode: "",        
                    actionName: "",
                    details: $"AssetCategories details was fetched.",
                    module:"AssetCategories"
                );
                await _mediator.Publish(domainEvent, cancellationToken);
            return new ApiResponseDTO<List<AssetCategoriesDto>> 
            { 
                IsSuccess = true, 
                Message = "Success", 
                Data = assetCategorieslist ,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
                };
        }
    }
}