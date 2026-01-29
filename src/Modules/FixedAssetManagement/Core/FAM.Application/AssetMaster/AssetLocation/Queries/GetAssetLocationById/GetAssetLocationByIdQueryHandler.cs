using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FAM.Application.AssetLocation.Queries.GetAssetLocation;
using FAM.Application.Common.HttpResponse;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetLocation;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;

namespace FAM.Application.AssetLocation.Queries.GetAssetLocationById
{
 
    public class GetAssetLocationByIdQueryHandler : IRequestHandler<GetAssetLocationByIdQuery, AssetLocationDto>
    {
        private readonly IAssetLocationQueryRepository _assetLocationRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator; 
        public GetAssetLocationByIdQueryHandler(IAssetLocationQueryRepository assetLocationRepository,  IMapper mapper, IMediator mediator)
        {
            _assetLocationRepository = assetLocationRepository;
            _mapper = mapper;
            _mediator = mediator;

        }

 
      public async Task<AssetLocationDto> Handle(GetAssetLocationByIdQuery request, CancellationToken cancellationToken)
        {
             var assetLocation = await _assetLocationRepository.GetByIdAsync(request.Id);
           

            var assetlocationDto = _mapper.Map<AssetLocationDto>(assetLocation);
         

            if (assetLocation is null)
            {       
                throw new ValidationException("AssetLocation with ID {request.Id} not found.");         
  
            }       
            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: assetlocationDto.AssetId == null ? "" : assetlocationDto.AssetId.ToString(),        
                actionName: assetlocationDto.SubLocationId == null ? "" : assetlocationDto.SubLocationId.ToString(),                
                details: $"Asset '{assetlocationDto.UnitId}' was created. Code: {assetlocationDto.DepartmentId}",
                module:"AssetMasterGeneral"
            );
            await _mediator.Publish(domainEvent, cancellationToken);
            return  assetlocationDto;       
        }

       
    }
}