using PartyManagement.Application.Common.Interfaces.IBankAccount;
using PartyManagement.Domain.Events;
using MediatR;

namespace PartyManagement.Application.BankAccount.Command.CreateBankAccount;


public class CreateBankAccountCommandHandler : IRequestHandler<CreateBankAccountCommand, int>
{
    private readonly IBankAccountCommandRepository _repo;
    private readonly IMediator _mediator;

    public CreateBankAccountCommandHandler(IBankAccountCommandRepository repo, IMediator mediator)
    {
        _repo = repo;
        _mediator = mediator;  
    } 
    public async Task<int> Handle(CreateBankAccountCommand r, CancellationToken ct)
    {
        var entity = new Domain.Entities.BankAccount
        {
            BankId = r.BankId,                        
            AccountNumber = r.AccountNumber.Trim(),
            BranchId = r.BranchId,            
            IFSCCode = r.IFSCCode?.Trim().ToUpperInvariant(),
            SWIFTCode = r.SWIFTCode?.Trim().ToUpperInvariant(),
            AccountTypeId = r.AccountTypeId,
            AccountHolderName = r.AccountHolderName.Trim(),
            IsDefaultAccount = r.IsDefaultAccount,
            IsPrimaryAccount = r.IsPrimaryAccount,
            IBan = r.IBan?.Replace(" ", string.Empty).ToUpperInvariant(),
            OwnerTypeId = r.OwnerTypeId,
            OwnerId = r.OwnerId,
            AddressLine1 = r.AddressLine1?.Trim(),
            AddressLine2 = r.AddressLine2?.Trim(),
            CityId = r.CityId,
            StateId = r.StateId,
            Pincode = r.Pincode?.Trim()
        };

        var created = await _repo.AddAsync(entity, ct);
        var ev = new AuditLogsDomainEvent(
            actionDetail: "Create",
            actionCode: created.AccountNumber ?? created.Id.ToString(),
            actionName: created.BankId.ToString() ?? "BankAccount",
            details: $"Bank account '{created.BankId} ' (Id={created.Id}) created.",
            module: "BankAccount"
        );
        await _mediator.Publish(ev, ct);

        return created.Id;
    }
}