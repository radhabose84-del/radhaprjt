
using PartyManagement.Application.BankAccount;
using PartyManagement.Application.BankAccount.Query.GetBankAutocomplete;
using PartyManagement.Domain.Entities;

namespace PartyManagement.Application.Common.Interfaces.IBankAccount;


public interface IBankAccountQueryRepository
{
    Task<BankAccountDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<(IReadOnlyList<BankAccountDto> Items, int Total)> GetAllAsync(
        int pageNumber,
        int pageSize,
        string? search,int? bankId,
        CancellationToken ct);
    Task<IReadOnlyList<BankLookupDto>> AutocompleteAsync(string SearchTerm, CancellationToken ct);         
}