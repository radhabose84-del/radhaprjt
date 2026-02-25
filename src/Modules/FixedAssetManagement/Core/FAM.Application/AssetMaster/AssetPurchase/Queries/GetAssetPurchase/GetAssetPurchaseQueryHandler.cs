using AutoMapper;
using Contracts.Common;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetPurchase;
using FAM.Domain.Events;
using MediatR;

namespace FAM.Application.AssetMaster.AssetPurchase.Queries.GetAssetPurchase
{
    public class GetAssetPurchaseQueryHandler  : IRequestHandler<GetAssetPurchaseQuery,ApiResponseDTO<List<AssetPurchaseDetailsDto>>>
    {
        
        private readonly IAssetPurchaseQueryRepository _iAssetPurchaseQueryRepository;          
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAssetPurchaseQueryHandler(IAssetPurchaseQueryRepository iAssetPurchaseQueryRepository, IMapper mapper, IMediator mediator)
        {
            _iAssetPurchaseQueryRepository = iAssetPurchaseQueryRepository;            
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<AssetPurchaseDetailsDto>>> Handle(GetAssetPurchaseQuery request, CancellationToken cancellationToken)
        {
            var (assetpurchase, totalCount) = await _iAssetPurchaseQueryRepository.GetAllPurchaseDetails(request.PageNumber, request.PageSize, request.SearchTerm);
            var assetpurchaselist = _mapper.Map<List<AssetPurchaseDetailsDto>>(assetpurchase);

             //Domain Event
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetAssetGroup",
                    actionCode: "",        
                    actionName: "",
                    details: $"AssetGroup details was fetched.",
                    module:"AssetGroup"
                );
                await _mediator.Publish(domainEvent, cancellationToken);
            return new ApiResponseDTO<List<AssetPurchaseDetailsDto>> 
            { 
                IsSuccess = true, 
                Message = "Success", 
                Data = assetpurchaselist ,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
                };
        }
    }
}