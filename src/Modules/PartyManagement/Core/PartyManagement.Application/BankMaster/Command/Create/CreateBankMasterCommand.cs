using MediatR;
namespace PartyManagement.Application.BankMaster.Command.Create;
public record CreateBankMasterCommand(CreateBankMasterDto Dto) : IRequest<int>;