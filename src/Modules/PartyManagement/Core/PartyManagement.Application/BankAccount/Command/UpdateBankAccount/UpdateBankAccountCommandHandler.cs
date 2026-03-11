using PartyManagement.Application.BankAccount.Command.UpdateBankAccount;
using Contracts.Interfaces;
using PartyManagement.Application.Common.Interfaces;
using PartyManagement.Application.Common.Interfaces.IBankAccount;
using PartyManagement.Domain.Events;
using MediatR;

namespace PartyManagement.Application.Party.BankAccounts.Commands.UpdateBankAccount
{
    public class UpdateBankAccountCommandHandler : IRequestHandler<UpdateBankAccountCommand, bool>
    {
        private readonly IBankAccountCommandRepository _repo;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMediator _mediator; // ← added

        public UpdateBankAccountCommandHandler(
            IBankAccountCommandRepository repo,
            IIPAddressService ipAddressService,
            IMediator mediator) // ← added
        {
            _repo = repo;
            _ipAddressService = ipAddressService;
            _mediator = mediator; // ← added
        }

        public async Task<bool> Handle(UpdateBankAccountCommand r, CancellationToken ct)
        {
            var entity = await _repo.FindAsync(r.Id, ct);
            if (entity is null) return false;

            entity.BankId         = r.BankId;            
            entity.AccountNumber    = r.AccountNumber.Trim();
            entity.AccountHolderName = r.AccountHolderName.Trim();
            entity.BranchId = r.BranchId;                        
            entity.IFSCCode         = r.IFSCCode?.Trim().ToUpperInvariant();
            entity.SWIFTCode        = r.SWIFTCode?.Trim().ToUpperInvariant();
            entity.AccountTypeId    = r.AccountTypeId;
            entity.IsDefaultAccount = r.IsDefaultAccount;
            entity.IsPrimaryAccount = r.IsPrimaryAccount;
            entity.IsActive        =r.IsActive;
            entity.IBan             = r.IBan?.Replace(" ", string.Empty).ToUpperInvariant();
            entity.ModifiedDate     = DateTime.UtcNow;
            entity.ModifiedByName   = _ipAddressService.GetUserName();
            entity.ModifiedBy       = _ipAddressService.GetUserId();
            entity.ModifiedIP       = _ipAddressService.GetSystemIPAddress();

            await _repo.UpdateAsync(entity, ct);

            // 🔹 Audit event
            var ev = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: entity.AccountNumber ?? entity.Id.ToString(),
                actionName: entity.BankId.ToString() ?? "BankAccount",
                details: $"Bank account (Id={entity.Id}) updated by {_ipAddressService.GetUserName()} from {_ipAddressService.GetSystemIPAddress()}.",
                module: "BankAccount"
            );
            await _mediator.Publish(ev, ct);

            return true;
        }
    }
}
