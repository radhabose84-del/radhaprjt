#nullable disable
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesGroup;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesGroup.Commands.DeleteSalesGroup
{
    public sealed class DeleteSalesGroupCommandHandler : IRequestHandler<DeleteSalesGroupCommand, bool>
    {
        private readonly ISalesGroupCommandRepository _commandRepo;
        private readonly ISalesGroupQueryRepository _queryRepo;
        private readonly IMediator _mediator;

        public DeleteSalesGroupCommandHandler(
            ISalesGroupCommandRepository commandRepo,
            ISalesGroupQueryRepository queryRepo,
            IMediator mediator)
        {
            _commandRepo = commandRepo;
            _queryRepo = queryRepo;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteSalesGroupCommand request, CancellationToken ct)
        {
            var before = await _queryRepo.GetByIdAsync(request.Id)
                         ?? throw new ExceptionRules("Sales Group not found.");

            var ok = await _commandRepo.SoftDeleteAsync(request.Id, ct);
            if (!ok) throw new ExceptionRules("Failed to delete Sales Group.");

            var ev = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "SALES_GROUP_DELETE",
                actionName: before.SalesGroupName,
                details: $"Sales Group '{before.SalesGroupName}' soft-deleted.",
                module: "SalesGroup"
            );
            await _mediator.Publish(ev, ct);

            return true;
        }
    }
}
