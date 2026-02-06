using PartyManagement.Application.BankAccount;
using MediatR;


namespace PartyManagement.Application.BankAccount.Query.GetBankAccountById;
public record GetBankAccountByIdQuery(int Id) : IRequest<BankAccountDto?>;