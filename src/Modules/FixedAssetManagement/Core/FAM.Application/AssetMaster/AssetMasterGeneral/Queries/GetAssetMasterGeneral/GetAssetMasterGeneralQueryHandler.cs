using AutoMapper;
using FAM.Application.Common.HttpResponse;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetMasterGeneral;
using FAM.Domain.Events;
using MediatR;

namespace FAM.Application.AssetMaster.AssetMasterGeneral.Queries.GetAssetMasterGeneral
{
    public class GetAssetMasterGeneralQueryHandler : IRequestHandler<GetAssetMasterGeneralQuery, ApiResponseDTO<List<AssetMasterGeneralDTO>>>
    {
        private readonly IAssetMasterGeneralQueryRepository _assetMasterRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator; 

        public GetAssetMasterGeneralQueryHandler(IAssetMasterGeneralQueryRepository assetMasterRepository , IMapper mapper, IMediator mediator)
        {
            _assetMasterRepository = assetMasterRepository;
            _mapper = mapper;
            _mediator = mediator;
        }
        public async Task<ApiResponseDTO<List<AssetMasterGeneralDTO>>> Handle(GetAssetMasterGeneralQuery request, CancellationToken cancellationToken)
        {
            var (assetMaster, totalCount) = await _assetMasterRepository.GetAllAssetAsync(request.PageNumber, request.PageSize, request.SearchTerm);
            var assetMasterList = _mapper.Map<List<AssetMasterGeneralDTO>>(assetMaster);

            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "",        
                actionName: "",
                details: $"DepreciationGroup details was fetched.",
                module:"DepreciationGroup"
            );
            await _mediator.Publish(domainEvent, cancellationToken);
            return new ApiResponseDTO<List<AssetMasterGeneralDTO>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = assetMasterList,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };            
        }
    }
}