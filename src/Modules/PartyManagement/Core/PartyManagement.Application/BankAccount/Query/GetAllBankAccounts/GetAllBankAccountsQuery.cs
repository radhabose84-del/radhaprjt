using MediatR;


namespace PartyManagement.Application.BankAccount.Query.GetBankAccountsPaged;
public record GetAllBankAccountsQuery( int PageNumber = 1, int PageSize = 20, string? Search = null,int? BankId = null)
: IRequest<(IReadOnlyList<BankAccountDto> Items, int Total)>;