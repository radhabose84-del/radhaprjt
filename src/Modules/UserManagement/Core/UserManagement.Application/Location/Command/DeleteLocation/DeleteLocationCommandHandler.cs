using AutoMapper;
using UserManagement.Application.Common.Interfaces.ILocation;
using UserManagement.Domain.Events;
using FluentValidation;
using MediatR;

namespace UserManagement.Application.Location.Command.DeleteLocation
{
    public class DeleteLocationCommandHandler : IRequestHandler<DeleteLocationCommand, bool>
    {
        private readonly ILocationCommandRepository _locationCommandRepository;
        private readonly IMapper _mapper;
        private readonly ILocationQueryRepository _locationQueryRepository;
        private readonly IMediator _mediator;

        public DeleteLocationCommandHandler(
            ILocationCommandRepository locationCommandRepository,
            ILocationQueryRepository locationQueryRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _locationCommandRepository = locationCommandRepository;
            _locationQueryRepository = locationQueryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<bool> Handle(DeleteLocationCommand request, CancellationToken cancellationToken)
        {
            var updatedLocation = _mapper.Map<UserManagement.Domain.Entities.Location>(request);
            var result = await _locationCommandRepository.DeleteAsync(request.Id, updatedLocation);

            if (!result)
            {
                throw new ValidationException("Failed to delete location.");
            }

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Delete",
                actionCode: request.Id.ToString(),
                actionName: "",
                details: $"Location ID: {request.Id} was changed to status inactive.",
                module: "Location");

            await _mediator.Publish(domainEvent, cancellationToken);

            return true;
        }
    }
}
