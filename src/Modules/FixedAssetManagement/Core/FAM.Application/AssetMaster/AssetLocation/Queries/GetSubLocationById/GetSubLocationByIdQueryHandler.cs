using AutoMapper;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetLocation;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;

namespace FAM.Application.AssetMaster.AssetLocation.Queries.GetSubLocationById
{
    public class GetSubLocationByIdQueryHandler  : IRequestHandler<GetSubLocationByIdQuery, List<GetAssetSubLocationDto>>
    {
         private readonly IAssetLocationQueryRepository _assetLocationRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator; 

         public GetSubLocationByIdQueryHandler(IAssetLocationQueryRepository assetLocationRepository,  IMapper mapper, IMediator mediator)
        {
            _assetLocationRepository = assetLocationRepository;
            _mapper = mapper;
            _mediator = mediator;

        }
        public async Task<List<GetAssetSubLocationDto>> Handle(GetSubLocationByIdQuery request, CancellationToken cancellationToken)
                {
                    var assetLocations = await _assetLocationRepository.GetSublocationByIdAsync(request.Id); // Fetch list

                    if (assetLocations == null ) // Check if empty
                    {
                        throw new ValidationException($"No AssetSub Locations found for ID {request.Id}."); 

                    }

                    var assetLocationsDto = _mapper.Map<List<GetAssetSubLocationDto>>(assetLocations); // Map list

                    // Domain Event
                    var domainEvent = new AuditLogsDomainEvent(
                        actionDetail: "GetAll",
                        actionCode: "",        
                        actionName: "", 
                        details: $"Asset Sub Location details were fetched for ID {request.Id}.",
                        module: "AssetSub Location"
                    );
                    await _mediator.Publish(domainEvent, cancellationToken);

                    return  assetLocationsDto;
                }
              
        
    }
}