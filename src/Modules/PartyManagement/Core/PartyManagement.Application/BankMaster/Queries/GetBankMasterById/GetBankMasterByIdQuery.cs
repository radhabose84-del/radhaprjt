using MediatR;

namespace PartyManagement.Application.BankMaster.Queries.GetBankMasterById;
public record GetBankMasterByIdQuery(int Id) : IRequest<BankMasterDto?>;