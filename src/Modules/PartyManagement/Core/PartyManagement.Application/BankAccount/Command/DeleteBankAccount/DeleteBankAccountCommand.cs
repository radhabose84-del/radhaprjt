using MediatR;


namespace PartyManagement.Application.BankAccount.Command.DeleteBankAccount;
public sealed record DeleteBankAccountCommand(int Id) : IRequest<bool>;