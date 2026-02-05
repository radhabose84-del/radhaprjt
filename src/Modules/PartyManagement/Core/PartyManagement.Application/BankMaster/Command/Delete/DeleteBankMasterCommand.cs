using MediatR;

namespace PartyManagement.Application.BankMaster.Command.Delete;

public record DeleteBankMasterCommand(int Id) : IRequest;

