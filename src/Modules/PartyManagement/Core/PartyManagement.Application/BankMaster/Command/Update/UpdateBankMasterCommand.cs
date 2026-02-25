using MediatR;

namespace PartyManagement.Application.BankMaster.Command.Update;
public record UpdateBankMasterCommand(UpdateBankMasterDto Dto) : IRequest;