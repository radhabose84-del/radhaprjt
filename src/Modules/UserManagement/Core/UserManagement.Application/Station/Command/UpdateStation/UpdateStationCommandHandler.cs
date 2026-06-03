using AutoMapper;
using UserManagement.Application.Common.Interfaces.IStation;
using UserManagement.Domain.Events;
using FluentValidation;
using MediatR;

namespace UserManagement.Application.Station.Command.UpdateStation
{
    public class UpdateStationCommandHandler : IRequestHandler<UpdateStationCommand, int>
    {
        private readonly IStationCommandRepository _stationCommandRepository;
        private readonly IMapper _mapper;
        private readonly IStationQueryRepository _stationQueryRepository;
        private readonly IMediator _mediator;

        public UpdateStationCommandHandler(
            IStationCommandRepository stationCommandRepository,
            IMapper mapper,
            IStationQueryRepository stationQueryRepository,
            IMediator mediator)
        {
            _stationCommandRepository = stationCommandRepository;
            _mapper = mapper;
            _stationQueryRepository = stationQueryRepository;
            _mediator = mediator;
        }

        public async Task<int> Handle(UpdateStationCommand request, CancellationToken cancellationToken)
        {
            var existing = await _stationQueryRepository.GetStationByIdAsync(request.Id);
            if (existing == null)
            {
                throw new ValidationException("Station not found.");
            }

            existing.StationName = request.StationName;
            existing.Description = request.Description;
            existing.IsActive = request.IsActive;

            var rows = await _stationCommandRepository.UpdateAsync(request.Id, existing);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: request.Id.ToString(),
                actionName: request.StationName ?? "",
                details: $"Station with Id {request.Id} was updated.",
                module: "Station");

            await _mediator.Publish(domainEvent, cancellationToken);

            return rows ? 1 : 0;
        }
    }
}
