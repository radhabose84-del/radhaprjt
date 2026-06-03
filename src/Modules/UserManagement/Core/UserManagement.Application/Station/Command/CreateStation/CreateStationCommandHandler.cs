using AutoMapper;
using UserManagement.Application.Common.Interfaces.IStation;
using UserManagement.Domain.Events;
using MediatR;

namespace UserManagement.Application.Station.Command.CreateStation
{
    public class CreateStationCommandHandler : IRequestHandler<CreateStationCommand, int>
    {
        private readonly IStationCommandRepository _stationCommandRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public CreateStationCommandHandler(
            IStationCommandRepository stationCommandRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _stationCommandRepository = stationCommandRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<int> Handle(CreateStationCommand request, CancellationToken cancellationToken)
        {
            var stationEntity = _mapper.Map<UserManagement.Domain.Entities.Station>(request);

            var result = await _stationCommandRepository.CreateAsync(stationEntity);

            if (result <= 0)
            {
                throw new Exception("Station creation failed");
            }

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: stationEntity.Code ?? "",
                actionName: stationEntity.StationName ?? "",
                details: $"Station '{stationEntity.StationName}' was created.",
                module: "Station");

            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
