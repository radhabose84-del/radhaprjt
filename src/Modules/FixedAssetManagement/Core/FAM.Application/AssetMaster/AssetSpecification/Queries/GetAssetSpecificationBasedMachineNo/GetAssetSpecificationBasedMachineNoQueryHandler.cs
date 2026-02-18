using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Common;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetSpecification;
using FAM.Domain.Events;
using MediatR;

namespace FAM.Application.AssetMaster.AssetSpecification.Queries.GetAssetSpecificationBasedMachineNo
{
    public class GetAssetSpecificationBasedMachineNoQueryHandler : IRequestHandler<GetAssetSpecificationBasedMachineNoQuery, ApiResponseDTO<List<AssetSpecBasedOnMachineNoDto>>>
    {
        private readonly IAssetSpecificationQueryRepository _assetSpecificationRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAssetSpecificationBasedMachineNoQueryHandler(IAssetSpecificationQueryRepository assetSpecificationRepository, IMapper mapper, IMediator mediator)
        {
            _assetSpecificationRepository = assetSpecificationRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<AssetSpecBasedOnMachineNoDto>>> Handle(GetAssetSpecificationBasedMachineNoQuery request, CancellationToken cancellationToken)
        {
            var (specList, totalCount) = await _assetSpecificationRepository.GetAssetSpecBasedOnMachineNos(request.PageNumber, request.PageSize, request.SearchTerm);

            var dtoList = _mapper.Map<List<AssetSpecBasedOnMachineNoDto>>(specList);

            // Domain Event for Auditing
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAssetSpecificationBasedOnMachineNo",
                actionCode: "FETCH",
                actionName: "Fetch Asset Specifications",
                details: $"Asset Specifications based on machine numbers were fetched.",
                module: "AssetSpecification"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<AssetSpecBasedOnMachineNoDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = dtoList,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }

        
    }
}