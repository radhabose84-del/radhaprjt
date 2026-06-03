using AutoMapper;
using UserManagement.Application.Common.Interfaces.IStation;
using UserManagement.Domain.Events;
using FluentValidation;
using MediatR;

namespace UserManagement.Application.Station.Queries.GetStationAutoSearch
{
    public class GetStationAutoCompleteQueryHandler : IRequestHandler<GetStationAutoCompleteQuery, List<StationAutoCompleteDto>>
    {
        private readonly IStationQueryRepository _stationRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetStationAutoCompleteQueryHandler(IStationQueryRepository stationRepository, IMapper mapper, IMediator mediator)
        {
            _stationRepository = stationRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<List<StationAutoCompleteDto>> Handle(GetStationAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _stationRepository.GetAllStationAsync(request.SearchPattern ?? string.Empty);
            if (result is null || result.Count is 0)
            {
                throw new ValidationException("No Station found matching the search pattern.");
            }

            var stationDto = _mapper.Map<List<StationAutoCompleteDto>>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAutoComplete",
                actionCode: "",
                actionName: request.SearchPattern ?? string.Empty,
                details: $"Station '{request.SearchPattern}' was searched",
                module: "Station");

            await _mediator.Publish(domainEvent, cancellationToken);

            return stationDto;
        }
    }
}
