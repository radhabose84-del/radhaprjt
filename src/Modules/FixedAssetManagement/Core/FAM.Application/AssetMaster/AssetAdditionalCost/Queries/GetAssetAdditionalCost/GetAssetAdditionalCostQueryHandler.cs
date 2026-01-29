using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FAM.Application.Common.HttpResponse;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetAdditionalCost;
using FAM.Domain.Events;
using MediatR;

namespace FAM.Application.AssetMaster.AssetAdditionalCost.Queries.GetAssetAdditionalCost
{
    public class GetAssetAdditionalCostQueryHandler : IRequestHandler<GetAssetAdditionalCostQuery,ApiResponseDTO<List<AssetAdditionalCostDto>>>
    {
        private readonly IAssetAdditionalCostQueryRepository _iAssetAdditionalCostQueryRepository;        
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAssetAdditionalCostQueryHandler(IAssetAdditionalCostQueryRepository iAssetAdditionalCostQueryRepository, IMapper mapper, IMediator mediator)
        {
            _iAssetAdditionalCostQueryRepository = iAssetAdditionalCostQueryRepository;            
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<AssetAdditionalCostDto>>> Handle(GetAssetAdditionalCostQuery request, CancellationToken cancellationToken)
        {
            var (assetadditionalcost, totalCount) = await _iAssetAdditionalCostQueryRepository.GetAllAdditionalCostGroupAsync(request.PageNumber, request.PageSize, request.SearchTerm);
            var assetAdditionalsgruplist = _mapper.Map<List<AssetAdditionalCostDto>>(assetadditionalcost);

             //Domain Event
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetAssetAdditionalCost",
                    actionCode: "GetAssetAdditionalCost",        
                    actionName: "GetAssetAdditionalCost",
                    details: $"AssetAdditionalCost details was fetched.",
                    module:"AssetAdditionalCost"
                );
                await _mediator.Publish(domainEvent, cancellationToken);
            return new ApiResponseDTO<List<AssetAdditionalCostDto>> 
            { 
                IsSuccess = true, 
                Message = "Success", 
                Data = assetAdditionalsgruplist ,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
                };
        }
    }
}