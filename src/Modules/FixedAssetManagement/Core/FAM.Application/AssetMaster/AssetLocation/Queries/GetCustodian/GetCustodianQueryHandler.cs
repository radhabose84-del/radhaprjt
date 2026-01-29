using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FAM.Application.Common.HttpResponse;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetLocation;
using FAM.Domain.Events;
using MediatR;

namespace FAM.Application.AssetMaster.AssetLocation.Queries.GetCustodian
{
    public class GetCustodianQueryHandler : IRequestHandler<GetCustodianQuery, ApiResponseDTO<List<GetCustodianDto>>>
    {
        private readonly IAssetLocationQueryRepository _assetLocationRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetCustodianQueryHandler( IAssetLocationQueryRepository assetLocationRepository , IMapper mapper, IMediator mediator)
        {
            _assetLocationRepository = assetLocationRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<GetCustodianDto>>> Handle(GetCustodianQuery request, CancellationToken cancellationToken)
        {           
            var (assetcustodian, totalCount) = await _assetLocationRepository.GetAllCustodianAsync(request.OldUnitId,  request.SearchEmployee);
            var assetMasterList = _mapper.Map<List<GetCustodianDto>>(assetcustodian);
            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "",        
                actionName: "",
                details: $"Custodian details was fetched.",
                module:"Custodian"
            );
            await _mediator.Publish(domainEvent, cancellationToken);
            return new ApiResponseDTO<List<GetCustodianDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = assetMasterList,
                TotalCount = totalCount
               
            };          
        }
    }
}