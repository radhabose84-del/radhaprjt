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
) : IRequest<int>; // returns new Id