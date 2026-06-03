using AutoMapper;
using UserManagement.Application.Common.Interfaces.ILocation;
using UserManagement.Domain.Events;
using FluentValidation;
using MediatR;

namespace UserManagement.Application.Location.Queries.GetLocationById
{
    public class GetLocationByIdQueryHandler : IRequestHandler<GetLocationByIdQuery, LocationByIdDto>
    {
        private readonly ILocationQueryRepository _locationRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetLocationByIdQueryHandler(ILocationQueryRepository locationRepository, IMapper mapper, IMediator mediator)
        {
            _locationRepository = locationRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<LocationByIdDto> Handle(GetLocationByIdQuery request, CancellationToken cancellationToken)
        {
            var location = await _locationRepository.GetLocationByIdAsync(request.Id);

            if (location == null)
            {
                throw new ValidationException("Location not found.");
            }

            var locationDto = _mapper.Map<LocationByIdDto>(location);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: locationDto.Id.ToString(),
                actionName: locationDto.LocationName ?? "",
                details: $"Location '{locationDto.LocationName}' was fetched. Id: {locationDto.Id}",
                module: "Location");

            await _mediator.Publish(domainEvent, cancellationToken);

            return locationDto;
        }
    }
}
