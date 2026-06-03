using AutoMapper;
using UserManagement.Application.Common.Interfaces.IStation;
using UserManagement.Domain.Events;
using FluentValidation;
using MediatR;

namespace UserManagement.Application.Station.Command.DeleteStation
{
    public class DeleteStationCommandHandler : IRequestHandler<DeleteStationCommand, bool>
    {
        private readonly IStationCommandRepository _stationCommandRepository;
        private readonly IMapper _mapper;
        private readonly IStationQueryRepository _stationQueryRepository;
        private readonly IMediator _mediator;

        public DeleteStationCommandHandler(
            IStationCommandRepository stationCommandRepository,
            IStationQueryRepository stationQueryRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _stationCommandRepository = stationCommandRepository;
            _stationQueryRepository = stationQueryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<bool> Handle(DeleteStationCommand request, CancellationToken cancellationToken)
        {
            var updatedStation = _mapper.Map<UserManagement.Domain.Entities.Station>(request);
            var result = await _stationCommandRepository.DeleteAsync(request.Id, updatedStation);

            if (!result)
            {
                throw new ValidationException("Failed to delete station.");
            }

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Delete",
                actionCode: request.Id.ToString(),
                actionName: "",
                details: $"Station ID: {request.Id} was changed to status inactive.",
                module: "Station");

            await _mediator.Publish(domainEvent, cancellationToken);

            return true;
        }
    }
}
