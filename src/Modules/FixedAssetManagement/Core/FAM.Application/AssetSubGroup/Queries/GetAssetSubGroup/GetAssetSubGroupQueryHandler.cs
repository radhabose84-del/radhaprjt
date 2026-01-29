using AutoMapper;
using FAM.Application.Common.HttpResponse;
using FAM.Application.Common.Interfaces.IAssetSubGroup;
using FAM.Domain.Events;
using MediatR;

namespace FAM.Application.AssetSubGroup.Queries.GetAssetSubGroup
{
    public class GetAssetSubGroupQueryHandler : IRequestHandler<GetAssetSubGroupQuery,ApiResponseDTO<List<AssetSubGroupDto>>>
    {
        private readonly IAssetSubGroupQueryRepository _iAssetSubGroupQueryRepository;        
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAssetSubGroupQueryHandler(IAssetSubGroupQueryRepository iAssetSubGroupQueryRepository, IMapper mapper, IMediator mediator)
        {
            _iAssetSubGroupQueryRepository = iAssetSubGroupQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<AssetSubGroupDto>>> Handle(GetAssetSubGroupQuery request, CancellationToken cancellationToken)
        {
            var (assetSubGroup, totalCount) = await _iAssetSubGroupQueryRepository.GetAllAssetSubGroupAsync(request.PageNumber, request.PageSize, request.SearchTerm);
            var assetSubGroupList = _mapper.Map<List<AssetSubGroupDto>>(assetSubGroup);

             //Domain Event
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetAssetSubGroup",
                    actionCode: "",        
                    actionName: "",
                    details: $"AssetSubGroup details was fetched.",
                    module:"AssetSubGroup"
                );
                await _mediator.Publish(domainEvent, cancellationToken);
            return new ApiResponseDTO<List<AssetSubGroupDto>> 
            { 
                IsSuccess = true, 
                Message = "Success", 
                Data = assetSubGroupList ,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
                };
        }
    }
}