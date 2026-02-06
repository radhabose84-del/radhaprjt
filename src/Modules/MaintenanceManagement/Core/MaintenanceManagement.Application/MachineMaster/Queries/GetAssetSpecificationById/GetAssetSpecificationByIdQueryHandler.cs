using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Interfaces.Lookups.FixedAssetManagement;
using MaintenanceManagement.Application.Common.HttpResponse;
using MaintenanceManagement.Application.Common.Interfaces.IMachineMaster;
using MediatR;

namespace MaintenanceManagement.Application.MachineMaster.Queries.GetAssetSpecificationById
{
    public class GetAssetSpecificationByIdQueryHandler : IRequestHandler<GetAssetSpecificationByIdQuery, ApiResponseDTO<List<AssetSpecificationByAssetIdDto>>>
    {
        private readonly IAssetSpecificationLookup _assetSpecificationLookup;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAssetSpecificationByIdQueryHandler(IMapper mapper, IMediator mediator, IAssetSpecificationLookup assetSpecificationLookup)
        {
            _mapper = mapper;
            _mediator = mediator;
            _assetSpecificationLookup = assetSpecificationLookup;
        }

    public async Task<ApiResponseDTO<List<AssetSpecificationByAssetIdDto>>> Handle(GetAssetSpecificationByIdQuery request, CancellationToken cancellationToken)
       {
            var assetSpecifications = await _assetSpecificationLookup.GetByAssetIdAsync(request.AssetId, cancellationToken);

            var filteredSpecifications = assetSpecifications
                .Select(d => new AssetSpecificationByAssetIdDto
                {
                    AssetId = d.AssetId,
                    SpecificationName = d.SpecificationName,
                    SpecificationValue = d.SpecificationValue,
                    CapitalizationDate = d.CapitalizationDate
                }).ToList();

            return new ApiResponseDTO<List<AssetSpecificationByAssetIdDto>>
            {
                IsSuccess = true,
                Message = filteredSpecifications.Any() ? "Specifications found." : "No specifications found for the asset.",
                Data = filteredSpecifications,
                TotalCount = filteredSpecifications.Count
            };
      }
    }
}
