using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FAM.Application.AssetSubCategories.Queries.GetAssetSubCategories;
using Contracts.Common;
using FAM.Application.Common.Interfaces.IAssetSubCategories;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;

namespace FAM.Application.AssetSubCategories.Queries.GetAssetSubCategoriesByCategoryId
{
    public class GetAssetSubCategoriesByCategoryIdQueryHandler : IRequestHandler<GetAssetSubCategoriesByCategoryIdQuery,List<AssetSubCategoriesAutoCompleteDto>>
    {
        
        private readonly IAssetSubCategoriesQueryRepository _iAssetSubCategoriesQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAssetSubCategoriesByCategoryIdQueryHandler(IAssetSubCategoriesQueryRepository iAssetSubCategoriesQueryRepository, IMapper mapper, IMediator mediator)
        {
            _iAssetSubCategoriesQueryRepository = iAssetSubCategoriesQueryRepository;            
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<List<AssetSubCategoriesAutoCompleteDto>> Handle(GetAssetSubCategoriesByCategoryIdQuery request, CancellationToken cancellationToken)
        {
             var result = await _iAssetSubCategoriesQueryRepository.GetSubcategoriesByAssetCategoryIdAsync(request.AssetCategoriesId);

            // Check if data exists
            if (result is null || !result.Any())
            {
                throw new ValidationException($"No records found for ID {request.AssetCategoriesId}.");
              
            }

            // Map list of results
            var assetSubCategoriesList = _mapper.Map<List<AssetSubCategoriesAutoCompleteDto>>(result);

            // Domain Event Logging
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "AssetCategoryIdBasedSubCategory",
                actionName: request.AssetCategoriesId.ToString(),
                details: $"Asset SubCategory details for ID {request.AssetCategoriesId} were fetched.",
                module: "AssetCategoryBasedSubCategory"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return  assetSubCategoriesList;
        }
    }
}