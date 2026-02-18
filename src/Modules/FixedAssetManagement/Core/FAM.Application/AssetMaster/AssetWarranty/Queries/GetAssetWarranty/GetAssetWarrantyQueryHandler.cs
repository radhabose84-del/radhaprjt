using AutoMapper;
using Contracts.Common;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetWarranty;
using FAM.Domain.Events;
using MediatR;

namespace FAM.Application.AssetMaster.AssetWarranty.Queries.GetAssetWarranty
{
    public class GetAssetWarrantyQueryHandler : IRequestHandler<GetAssetWarrantyQuery, ApiResponseDTO<List<AssetWarrantyDTO>>>
    {
        private readonly IAssetWarrantyQueryRepository _assetWarrantyRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator; 

        public GetAssetWarrantyQueryHandler(IAssetWarrantyQueryRepository assetWarrantyRepository , IMapper mapper, IMediator mediator)
        {
            _assetWarrantyRepository = assetWarrantyRepository;
            _mapper = mapper;
            _mediator = mediator;
        }        
        public async Task<ApiResponseDTO<List<AssetWarrantyDTO>>> Handle(GetAssetWarrantyQuery request, CancellationToken cancellationToken)
        {
            var (assetWarranty, totalCount) = await _assetWarrantyRepository.GetAllAssetWarrantyAsync(request.PageNumber, request.PageSize, request.SearchTerm);
            var assetWarrantyList = _mapper.Map<List<AssetWarrantyDTO>>(assetWarranty);

            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "",        
                actionName: "",
                details: $"Asset Warranty details was fetched.",
                module:"Asset Warranty"
            );
            await _mediator.Publish(domainEvent, cancellationToken);
            return new ApiResponseDTO<List<AssetWarrantyDTO>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = assetWarrantyList,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };            
        }
    }
}