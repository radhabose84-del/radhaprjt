using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FAM.Application.AssetCategories.Queries.GetAssetCategories;
using FAM.Application.Common.HttpResponse;
using FAM.Application.Common.Interfaces.IAssetCategories;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;

namespace FAM.Application.AssetCategories.Queries.GetAssetCategoriesByAssetGroupId
{
    public class GetAssetCategoriesByAssetGroupIdQueryHandler : IRequestHandler<GetAssetCategoriesByAssetGroupIdQuery,List<AssetCategoriesAutoCompleteDto>>
    {
        private readonly IAssetCategoriesQueryRepository _iAssetCategoriesQueryRepository;        
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAssetCategoriesByAssetGroupIdQueryHandler(IAssetCategoriesQueryRepository iAssetCategoriesQueryRepository, IMapper mapper, IMediator mediator)
        {
            _iAssetCategoriesQueryRepository = iAssetCategoriesQueryRepository;            
            _mapper = mapper;
            _mediator = mediator;
        }


        public async Task<List<AssetCategoriesAutoCompleteDto>> Handle(GetAssetCategoriesByAssetGroupIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _iAssetCategoriesQueryRepository.GetByAssetgroupIdAsync(request.AssetGroupId);

            // Check if data exists
            if (result is null || !result.Any())
            {
                throw new ValidationException($"No records found for ID {request.AssetGroupId}.");
               
            }

            // Map list of results
            var assetTransferReceiptList = _mapper.Map<List<AssetCategoriesAutoCompleteDto>>(result);

            // Domain Event Logging
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "AssetGroupIdBasedCategory",
                actionName: request.AssetGroupId.ToString(),
                details: $"Asset Category details for ID {request.AssetGroupId} were fetched.",
                module: "AssetGroupIdBasedCategory"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return assetTransferReceiptList;
        }
    }
}