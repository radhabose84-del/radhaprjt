using UserManagement.Domain.Entities;
using MediatR;
using AutoMapper;
using UserManagement.Application.Common.Interfaces.IDivision;
using UserManagement.Domain.Events;

namespace UserManagement.Application.Divisions.Commands.DeleteDivision
{
    public class DeleteDivisionCommandHandler : IRequestHandler<DeleteDivisionCommand, bool>
    {
        private readonly IDivisionCommandRepository _divisionRepository;
        private readonly IMapper _imapper;
        private readonly IMediator _mediator;
        public DeleteDivisionCommandHandler(IDivisionCommandRepository divisionRepository, IMapper imapper , IMediator mediator)
        {
            _divisionRepository = divisionRepository;
            _imapper = imapper;
            _mediator = mediator;
        }
         public async Task<bool> Handle(DeleteDivisionCommand request, CancellationToken cancellationToken)
        {
            var division  = _imapper.Map<Division>(request);
            var divisionresult = await _divisionRepository.DeleteAsync(request.Id, division);

            // Idempotent delete: only audit when a row was actually soft-deleted.
            // A no-op (already deleted / non-existent id) returns false → controller 200, not a 500.
            if (divisionresult)
            {
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "Delete",
                    actionCode: division.Id.ToString(),
                    actionName: division.Id.ToString(),
                    details: $"Division '{division.Id}' was deleted.",
                    module:"Division"
                );
                await _mediator.Publish(domainEvent, cancellationToken);
            }

            return divisionresult;
        }
    }
}