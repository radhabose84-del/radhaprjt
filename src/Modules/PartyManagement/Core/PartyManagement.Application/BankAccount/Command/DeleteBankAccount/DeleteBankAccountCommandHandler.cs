using Contracts.Common; // if you use ExceptionRules
using PartyManagement.Application.Common.Interfaces.IBankAccount;
using PartyManagement.Domain.Events;
using MediatR;

namespace PartyManagement.Application.BankAccount.Command.DeleteBankAccount
{
    public sealed class DeleteBankAccountCommandHandler : IRequestHandler<DeleteBankAccountCommand, bool>
    {
        private readonly IBankAccountCommandRepository _repo;
        private readonly IMediator _mediator;

        public DeleteBankAccountCommandHandler(IBankAccountCommandRepository repo, IMediator mediator)
        {
            _repo = repo;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteBankAccountCommand r, CancellationToken ct)
        {
            var entity = await _repo.FindAsync(r.Id, ct);
            if (entity is null)
            {
                throw new ExceptionRules("Bank account not found.");
            }

            await _repo.SoftDeleteAsync(entity.Id,ct);

            // Audit domain event
            var ev = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: entity.AccountNumber ?? entity.Id.ToString(),
                actionName: entity.BankId.ToString() ?? "BankAccount",
                details: $"Bank account (Id={entity.Id}) ",
                module: "BankAccount"
            );
            await _mediator.Publish(ev, ct);

            return true;
        }
    }
}
