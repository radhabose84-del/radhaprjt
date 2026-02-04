using InventoryManagement.Application.Common.Exceptions;
using InventoryManagement.Application.Common.Interfaces.Item.PutAway;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.Application.Item.PutAway.Commands.DeletePutAwayRule
{
    public sealed class DeletePutAwayRuleCommandHandler : IRequestHandler<DeletePutAwayRuleCommand, int>
    {
        private readonly IPutAwayRuleCommandRepository _repo;
        private readonly IMediator _mediator;

        public DeletePutAwayRuleCommandHandler(IPutAwayRuleCommandRepository repo, IMediator mediator)
        {
            _repo = repo;
            _mediator = mediator;
        }

        public async Task<int> Handle(DeletePutAwayRuleCommand request, CancellationToken ct)
        {
            // Load entity (tracked) and ensure it exists
            var entity = await _repo.GetByIdAsync(request.Id, track: true, ct)
                ?? throw new EntityNotFoundException(nameof(DeletePutAwayRuleCommand), request.Id);
            // Soft delete
            await _repo.SoftDeleteAsync(entity, ct);

            // Audit log domain event
            var ev = new AuditLogsDomainEvent(
                actionDetail: "Delete",
                actionCode: "DeletePutAwayRule",
                actionName: request.Id.ToString(),
                details: $"Put-away rule {request.Id} soft-deleted.",
                module: "PutAway"
            );
            await _mediator.Publish(ev, ct);

            // Return the deleted id (matches IRequest<int>)
            return request.Id;
        }
    }
}
