using MediatR;

namespace PurchaseManagement.Application.PurchaseOrder.ContractPO.Command.Foreclose;

public sealed record ForecloseContractPOCommand(int Id) : IRequest<bool>;
