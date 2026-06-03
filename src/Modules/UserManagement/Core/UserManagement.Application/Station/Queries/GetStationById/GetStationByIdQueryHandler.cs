using AutoMapper;
using UserManagement.Application.Common.Interfaces.IStation;
using UserManagement.Domain.Events;
using FluentValidation;
using MediatR;

namespace UserManagement.Application.Station.Queries.GetStationById
{
    public class GetStationByIdQueryHandler : IRequestHandler<GetStationByIdQuery, StationByIdDto>
    {
        private readonly IStationQueryRepository _stationRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetStationByIdQueryHandler(IStationQueryRepository stationRepository, IMapper mapper, IMediator mediator)
        {
            _stationRepository = stationRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<StationByIdDto> Handle(GetStationByIdQuery request, CancellationToken cancellationToken)
        {
            var station = await _stationRepository.GetStationByIdAsync(request.Id);

            if (station == null)
            {
                throw new ValidationException("Station not found.");
            }

            var stationDto = _mapper.Map<StationByIdDto>(station);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: stationDto.Id.ToString(),
                actionName: stationDto.StationName ?? "",
                details: $"Station '{stationDto.StationName}' was fetched. Id: {stationDto.Id}",
                module: "Station");

            await _mediator.Publish(domainEvent, cancellationToken);

            return stationDto;
        }
    }
}
