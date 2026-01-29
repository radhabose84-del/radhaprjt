using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FAM.Application.Common.HttpResponse;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetLocation;
using FAM.Application.Common.Interfaces.ILocation;
using FAM.Application.Location.Queries.GetLocations;
using FAM.Domain.Common;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;

namespace FAM.Application.Location.Command.DeleteLocation
{
    public class DeleteLocationCommandHandler : IRequestHandler<DeleteLocationCommand, LocationDto>
    {
        private readonly ILocationCommandRepository _locationCommandRepository;
        private readonly ILocationQueryRepository _locationQueryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly IAssetLocationQueryRepository _assetLocationQueryRepository;
        public DeleteLocationCommandHandler(ILocationCommandRepository locationCommandRepository, IMediator mediator, IMapper mapper, ILocationQueryRepository locationQueryRepository, IAssetLocationQueryRepository assetLocationQueryRepository)
        {
            _locationCommandRepository = locationCommandRepository;
            _locationQueryRepository = locationQueryRepository;
            _mediator = mediator;
            _mapper = mapper;
            _assetLocationQueryRepository = assetLocationQueryRepository;
        }
        public async Task<LocationDto> Handle(DeleteLocationCommand request, CancellationToken cancellationToken)
        {
            var locations = await _locationQueryRepository.GetByIdAsync(request.Id);
            if (locations is null || locations.IsDeleted is BaseEntity.IsDelete.Deleted)
            {
                throw new ValidationException("Invalid LocationID.The specified Location does not exist or is inactive.  ");
              
            }
            var (assetLocations, _) = await _assetLocationQueryRepository.GetAllAssetLocationAsync(1, int.MaxValue, null);
            if (assetLocations.Any(a => a.LocationId == request.Id))
            {
                throw new ValidationException("Location already exists for this assetlocation.Cannot delete the Location.");
             
            }
            var locationdelete = _mapper.Map<FAM.Domain.Entities.Location>(request);
            var locationresult = await _locationCommandRepository.DeleteAsync(request.Id, locationdelete);
            if (locationresult > 0)
            {
                var locationDto = _mapper.Map<LocationDto>(locationdelete);
                //Domain Event  
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "Delete",
                    actionCode: locationdelete.Id.ToString(),
                    actionName: locationdelete.Id.ToString(),
                    details: $"Location '{locationdelete.Id}' was deleted.",
                    module: "Location"
                );
                await _mediator.Publish(domainEvent, cancellationToken);
                return  locationDto;
            }
            throw new Exception("Location deletion failed.");
      
        }
    }
}