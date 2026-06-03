using AutoMapper;
using UserManagement.Application.Common.Interfaces.ILocation;
using UserManagement.Domain.Events;
using FluentValidation;
using MediatR;

namespace UserManagement.Application.Location.Command.UpdateLocation
{
    public class UpdateLocationCommandHandler : IRequestHandler<UpdateLocationCommand, int>
    {
        private readonly ILocationCommandRepository _locationCommandRepository;
        private readonly IMapper _mapper;
        private readonly ILocationQueryRepository _locationQueryRepository;
        private readonly IMediator _mediator;

        public UpdateLocationCommandHandler(
            ILocationCommandRepository locationCommandRepository,
            IMapper mapper,
            ILocationQueryRepository locationQueryRepository,
            IMediator mediator)
        {
            _locationCommandRepository = locationCommandRepository;
            _mapper = mapper;
            _locationQueryRepository = locationQueryRepository;
            _mediator = mediator;
        }

        public async Task<int> Handle(UpdateLocationCommand request, CancellationToken cancellationToken)
        {
            var existing = await _locationQueryRepository.GetLocationByIdAsync(request.Id);
            if (existing == null)
            {
                throw new ValidationException("Location not found.");
            }

            existing.LocationName = request.LocationName;
            existing.Description = request.Description;
            existing.IsActive = request.IsActive;

            var rows = await _locationCommandRepository.UpdateAsync(request.Id, existing);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: request.Id.ToString(),
                actionName: request.LocationName ?? "",
                details: $"Location with Id {request.Id} was updated.",
                module: "Location");

            await _mediator.Publish(domainEvent, cancellationToken);

            return rows ? 1 : 0;
        }
    }
}
