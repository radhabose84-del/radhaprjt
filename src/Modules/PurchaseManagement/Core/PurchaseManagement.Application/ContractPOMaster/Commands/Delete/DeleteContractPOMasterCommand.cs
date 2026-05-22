using MediatR;

namespace PurchaseManagement.Application.ContractPOMaster.Commands.Delete;

public sealed record DeleteContractPOMasterCommand(int Id) : IRequest<bool>;
