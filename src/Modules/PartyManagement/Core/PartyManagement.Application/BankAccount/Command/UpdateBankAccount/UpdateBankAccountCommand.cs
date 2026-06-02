using PartyManagement.Domain.Common;
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
    BaseEntity.Status IsActive,
    int? OwnerTypeId = null,
    int? OwnerId = null,
    string? AddressLine1 = null,
    string? AddressLine2 = null,
    int? CityId = null,
    int? StateId = null,
    string? Pincode = null
) : IRequest<bool>;