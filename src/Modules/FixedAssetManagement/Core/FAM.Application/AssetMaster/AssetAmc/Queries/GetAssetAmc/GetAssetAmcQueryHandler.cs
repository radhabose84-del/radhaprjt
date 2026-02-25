using AutoMapper;
using Contracts.Common;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetAmc;
using FAM.Domain.Events;
using MediatR;

namespace FAM.Application.AssetMaster.AssetAmc.Queries.GetAssetAmc
{
    public class GetAssetAmcQueryHandler : IRequestHandler<GetAssetAmcQuery,ApiResponseDTO<List<AssetAmcDto>>>
    {
        private readonly IAssetAmcQueryRepository _iAssetAmcQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

          public GetAssetAmcQueryHandler(IAssetAmcQueryRepository iAssetAmcQueryRepository, IMapper mapper,IMediator mediator)
        {
            _iAssetAmcQueryRepository = iAssetAmcQueryRepository;
            _mapper = mapper;
            _mediator=mediator;
        }

        public async Task<ApiResponseDTO<List<AssetAmcDto>>> Handle(GetAssetAmcQuery request, CancellationToken cancellationToken)
        {
            var (assetamc, totalCount) = await _iAssetAmcQueryRepository.GetAllAssetAmcAsync(request.PageNumber, request.PageSize, request.SearchTerm);
            var assetamclist = _mapper.Map<List<AssetAmcDto>>(assetamc);

             //Domain Event
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetAssetAmc",
                    actionCode: "GetAmc",        
                    actionName: "GetAmcDetails",
                    details: $"AssetAmc details was fetched.",
                    module:"AssetAmc"
                );
                await _mediator.Publish(domainEvent, cancellationToken);
            return new ApiResponseDTO<List<AssetAmcDto>> 
            { 
                IsSuccess = true, 
                Message = "Success", 
                Data = assetamclist ,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
                };
        }
    }
}