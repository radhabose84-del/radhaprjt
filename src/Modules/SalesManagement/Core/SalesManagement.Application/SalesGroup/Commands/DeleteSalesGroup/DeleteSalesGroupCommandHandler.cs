using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesGroup;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesGroup.Commands.DeleteSalesGroup
{
    public sealed class DeleteSalesGroupCommandHandler : IRequestHandler<DeleteSalesGroupCommand, bool>
    {
        private readonly ISalesGroupCommandRepository _commandRepo;
        private readonly IMediator _mediator;

        public DeleteSalesGroupCommandHandler(
            ISalesGroupCommandRepository commandRepo,
            IMediator mediator)
        {
            _commandRepo = commandRepo;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteSalesGroupCommand request, CancellationToken ct)
        {
            await _commandRepo.SoftDeleteAsync(request.Id, ct);

            var ev = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "SALES_GROUP_DELETE",
                actionName: request.Id.ToString(),
                details: $"Sales Group with Id {request.Id} soft-deleted.",
                module: "SalesGroup"
            );
            await _mediator.Publish(ev, ct);

            return true;
        }
    }
}
