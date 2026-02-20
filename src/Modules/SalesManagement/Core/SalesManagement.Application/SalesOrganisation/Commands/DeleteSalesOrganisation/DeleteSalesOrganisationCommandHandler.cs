#nullable disable
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesOrganisation;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesOrganisation.Commands.DeleteSalesOrganisation
{
    public sealed class DeleteSalesOrganisationCommandHandler
        : IRequestHandler<DeleteSalesOrganisationCommand, bool>
    {
        private readonly ISalesOrganisationCommandRepository _commandRepo;
        private readonly ISalesOrganisationQueryRepository _queryRepo;
        private readonly IMediator _mediator;

        public DeleteSalesOrganisationCommandHandler(
            ISalesOrganisationCommandRepository commandRepo,
            ISalesOrganisationQueryRepository queryRepo,
            IMediator mediator)
        {
            _commandRepo = commandRepo;
            _queryRepo = queryRepo;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteSalesOrganisationCommand request, CancellationToken ct)
        {
            var before = await _queryRepo.GetByIdAsync(request.Id)
                         ?? throw new ExceptionRules("Sales Organisation not found.");

            var ok = await _commandRepo.SoftDeleteAsync(request.Id, ct);
            if (!ok) throw new ExceptionRules("Failed to delete Sales Organisation.");

            var ev = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: before.SalesOrganisationCode,
                actionName: before.SalesOrganisationName,
                details: $"Sales Organisation '{before.SalesOrganisationCode} - {before.SalesOrganisationName}' soft-deleted.",
                module: "SalesOrganisation"
            );
            await _mediator.Publish(ev, ct);

            return true;
        }
    }
}
