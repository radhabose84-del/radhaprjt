using MediatR;

namespace PurchaseManagement.Application.PurchaseOrder.ContractPO.Command.Cancel;

public sealed record CancelContractPOCommand(int Id) : IRequest<bool>;
