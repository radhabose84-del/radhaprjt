using MediatR;

namespace PurchaseManagement.Application.ContractPO.Commands.Delete;

public sealed record DeleteContractPOCommand(int Id) : IRequest<bool>;
