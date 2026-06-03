using AutoMapper;
using UserManagement.Application.Common.Interfaces.ILocation;
using UserManagement.Domain.Events;
using FluentValidation;
using MediatR;

namespace UserManagement.Application.Location.Queries.GetLocationAutoSearch
{
    public class GetLocationAutoCompleteQueryHandler : IRequestHandler<GetLocationAutoCompleteQuery, List<LocationAutoCompleteDto>>
    {
        private readonly ILocationQueryRepository _locationRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetLocationAutoCompleteQueryHandler(ILocationQueryRepository locationRepository, IMapper mapper, IMediator mediator)
        {
            _locationRepository = locationRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<List<LocationAutoCompleteDto>> Handle(GetLocationAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _locationRepository.GetAllLocationAsync(request.SearchPattern ?? string.Empty);
            if (result is null || result.Count is 0)
            {
                throw new ValidationException("No Location found matching the search pattern.");
            }

            var locationDto = _mapper.Map<List<LocationAutoCompleteDto>>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAutoComplete",
                actionCode: "",
                actionName: request.SearchPattern ?? string.Empty,
                details: $"Location '{request.SearchPattern}' was searched",
                module: "Location");

            await _mediator.Publish(domainEvent, cancellationToken);

            return locationDto;
        }
    }
}
