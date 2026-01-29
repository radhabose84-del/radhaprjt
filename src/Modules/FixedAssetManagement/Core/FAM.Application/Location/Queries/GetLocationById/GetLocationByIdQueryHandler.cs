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

namespace FAM.Application.Location.Queries.GetLocationById
{
    public class GetLocationByIdQueryHandler : IRequestHandler<GetLocationByIdQuery, LocationDto>
    {
        private readonly ILocationQueryRepository _locationQueryRepository;        
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        public GetLocationByIdQueryHandler(ILocationQueryRepository locationQueryRepository ,IMediator mediator,IMapper mapper)        {
            _locationQueryRepository = locationQueryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }
        public async Task<LocationDto> Handle(GetLocationByIdQuery request, CancellationToken cancellationToken)
        {
           var result = await _locationQueryRepository.GetByIdAsync(request.Id);
            if (result is null)
            {
                throw new ValidationException("LocationId not found");
                
            }  
           var location = _mapper.Map<LocationDto>(result);

          //Domain Event
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetById",
                    actionCode: "",        
                    actionName: "",
                    details: $"Location details {location.Id} was fetched.",
                    module:"Location"
                );
                await _mediator.Publish(domainEvent, cancellationToken);
          return location;
        }
    }
}