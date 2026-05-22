using Contracts.Common;
using MediatR;


namespace PartyManagement.Application.BankAccount.Command.CreateBankAccount;
public record CreateBankAccountCommand(                
        int BankId,        
        string AccountNumber,
        string AccountHolderName,
        int BranchId,
        string? IFSCCode,
        string? SWIFTCode,
        int AccountTypeId,
        bool IsDefaultAccount,
        bool IsPrimaryAccount,
        string? IBan
) : IRequest<int>, IRequirePermission
{
    public PermissionType RequiredPermission => PermissionType.CanAdd;
} // returns new Id