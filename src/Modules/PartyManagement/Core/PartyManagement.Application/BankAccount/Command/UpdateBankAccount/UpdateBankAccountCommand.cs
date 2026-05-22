using PartyManagement.Domain.Common;
using Contracts.Common;
using MediatR;

namespace PartyManagement.Application.BankAccount.Command.UpdateBankAccount;
public record UpdateBankAccountCommand(
    int Id,
    int BankId,    
    string AccountNumber,
    string AccountHolderName,
    int BranchId,    
    string? IFSCCode,
    string? SWIFTCode,
    int AccountTypeId,
    bool IsDefaultAccount,
    bool IsPrimaryAccount,
    string? IBan,
    BaseEntity.Status IsActive
) : IRequest<bool>, IRequirePermission
{
    public PermissionType RequiredPermission => PermissionType.CanUpdate;
}