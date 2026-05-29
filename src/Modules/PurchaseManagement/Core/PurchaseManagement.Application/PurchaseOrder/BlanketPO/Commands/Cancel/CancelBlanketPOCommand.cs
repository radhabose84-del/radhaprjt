using MediatR;

namespace PurchaseManagement.Application.PurchaseOrder.BlanketPO.Commands.Cancel;

public sealed record CancelBlanketPOCommand(int Id) : IRequest<bool>;
