using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FAM.Application.Common.HttpResponse;
using FAM.Application.Common.Interfaces.ILocation;
using FAM.Application.Location.Queries.GetLocations;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;

namespace FAM.Application.Location.Command.CreateLocation
{
    public class CreateLocationCommandHandler : IRequestHandler<CreateLocationCommand, LocationDto>
    {
        private readonly ILocationCommandRepository _locationCommandRepository;
        private readonly ILocationQueryRepository _locationQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        public CreateLocationCommandHandler(ILocationCommandRepository locationCommandRepository,ILocationQueryRepository locationQueryRepository,IMapper mapper,IMediator mediator)
        {
            _locationCommandRepository = locationCommandRepository;
            _locationQueryRepository = locationQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }
        public async Task<LocationDto> Handle(CreateLocationCommand request, CancellationToken cancellationToken)
        {
               var existingLocation = await _locationQueryRepository.GetByLocationNameAsync(request.LocationName, request.DepartmentId,request.UnitId);

               if (existingLocation != null)
               {
                throw new ValidationException("Location already exists");
                   
               }
           
                 var location  = _mapper.Map<FAM.Domain.Entities.Location>(request);

                var locationresult = await _locationCommandRepository.CreateAsync(location);
                
                var locationMap = _mapper.Map<LocationDto>(locationresult);
                if (locationresult.Id > 0)
                {
                    var domainEvent = new AuditLogsDomainEvent(
                     actionDetail: "Create",
                     actionCode: locationresult.Code,
                     actionName: locationresult.LocationName,
                     details: $"Location '{locationresult.Code}' was created. LocationName: {locationresult.LocationName}",
                     module:"Location"
                 );
                 await _mediator.Publish(domainEvent, cancellationToken);
                 
                    return locationMap;
                }
               throw new Exception("Location not created");
                    
        }
    }
}