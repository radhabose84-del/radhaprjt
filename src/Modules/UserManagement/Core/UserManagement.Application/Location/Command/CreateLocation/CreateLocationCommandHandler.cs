using AutoMapper;
using UserManagement.Application.Common.Interfaces.ILocation;
using UserManagement.Domain.Events;
using MediatR;

namespace UserManagement.Application.Location.Command.CreateLocation
{
    public class CreateLocationCommandHandler : IRequestHandler<CreateLocationCommand, int>
    {
        private readonly ILocationCommandRepository _locationCommandRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public CreateLocationCommandHandler(
            ILocationCommandRepository locationCommandRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _locationCommandRepository = locationCommandRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<int> Handle(CreateLocationCommand request, CancellationToken cancellationToken)
        {
            var locationEntity = _mapper.Map<UserManagement.Domain.Entities.Location>(request);

            var result = await _locationCommandRepository.CreateAsync(locationEntity);

            if (result <= 0)
            {
                throw new Exception("Location creation failed");
            }

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: locationEntity.Code ?? "",
                actionName: locationEntity.LocationName ?? "",
                details: $"Location '{locationEntity.LocationName}' was created.",
                module: "Location");

            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
