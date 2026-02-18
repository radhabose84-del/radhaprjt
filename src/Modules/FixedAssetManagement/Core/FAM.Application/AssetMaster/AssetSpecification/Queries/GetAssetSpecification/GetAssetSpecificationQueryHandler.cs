using AutoMapper;
using Contracts.Common;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetSpecification;
using FAM.Domain.Events;
using MediatR;

namespace FAM.Application.AssetMaster.AssetSpecification.Queries.GetAssetSpecification
{    
    public class GetAssetSpecificationQueryHandler : IRequestHandler<GetAssetSpecificationQuery, ApiResponseDTO<List<AssetSpecificationJsonDto>>>
    {
        private readonly IAssetSpecificationQueryRepository _assetSpecificationRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator; 

        public GetAssetSpecificationQueryHandler(IAssetSpecificationQueryRepository assetSpecificationRepository , IMapper mapper, IMediator mediator)
        {
            _assetSpecificationRepository = assetSpecificationRepository;
            _mapper = mapper;
            _mediator = mediator;
        }        
        public async Task<ApiResponseDTO<List<AssetSpecificationJsonDto>>> Handle(GetAssetSpecificationQuery request, CancellationToken cancellationToken)
        {
            var (assetSpecification, totalCount) = await _assetSpecificationRepository.GetAllAssetSpecificationAsync(request.PageNumber, request.PageSize, request.SearchTerm);
            var assetSpecificationList = _mapper.Map<List<AssetSpecificationJsonDto>>(assetSpecification);

            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "",        
                actionName: "",
                details: $"Asset Specification details was fetched.",
                module:"Asset Specification"
            );
            await _mediator.Publish(domainEvent, cancellationToken);
            return new ApiResponseDTO<List<AssetSpecificationJsonDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = assetSpecificationList,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };            
        }
    }
}