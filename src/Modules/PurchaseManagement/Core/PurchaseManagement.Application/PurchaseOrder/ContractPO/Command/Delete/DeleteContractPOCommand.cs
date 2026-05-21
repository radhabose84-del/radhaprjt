using MediatR;

namespace PurchaseManagement.Application.PurchaseOrder.ContractPO.Command.Delete;

public sealed record DeleteContractPOCommand(int Id) : IRequest<bool>;
