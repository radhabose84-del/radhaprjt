using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Common;
using FAM.Application.Common.Interfaces.IAssetGroup;
using FAM.Domain.Events;
using MediatR;

namespace FAM.Application.AssetGroup.Queries.GetAssetGroup
{
    public class GetAssetGroupQueryHandler : IRequestHandler<GetAssetGroupQuery,ApiResponseDTO<List<AssetGroupDto>>>
    {
        private readonly IAssetGroupQueryRepository _iAssetGroupQueryRepository;        
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAssetGroupQueryHandler(IAssetGroupQueryRepository iAssetGroupQueryRepository, IMapper mapper, IMediator mediator)
        {
            _iAssetGroupQueryRepository = iAssetGroupQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<AssetGroupDto>>> Handle(GetAssetGroupQuery request, CancellationToken cancellationToken)
        {
            var (assetgroup, totalCount) = await _iAssetGroupQueryRepository.GetAllAssetGroupAsync(request.PageNumber, request.PageSize, request.SearchTerm);
            var assetgrouplist = _mapper.Map<List<AssetGroupDto>>(assetgroup);

             //Domain Event
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetAssetGroup",
                    actionCode: "",        
                    actionName: "",
                    details: $"AssetGroup details was fetched.",
                    module:"AssetGroup"
                );
                await _mediator.Publish(domainEvent, cancellationToken);
            return new ApiResponseDTO<List<AssetGroupDto>> 
            { 
                IsSuccess = true, 
                Message = "Success", 
                Data = assetgrouplist ,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
                };
        }
    }
}