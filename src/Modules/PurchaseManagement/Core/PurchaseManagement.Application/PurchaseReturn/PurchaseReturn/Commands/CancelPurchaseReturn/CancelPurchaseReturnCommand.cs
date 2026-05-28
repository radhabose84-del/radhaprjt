using MediatR;

namespace PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Commands.CancelPurchaseReturn;

public sealed record CancelPurchaseReturnCommand(int Id) : IRequest<bool>;
