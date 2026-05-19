using MediatR;

namespace PurchaseManagement.Application.PurchaseOrder.ContractPO.Command.Delete;

public sealed record DeleteContractReleasePOCommand(int Id) : IRequest<bool>;
