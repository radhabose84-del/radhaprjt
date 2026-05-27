using MediatR;

namespace PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Commands.DeletePurchaseReturn;

public sealed record DeletePurchaseReturnCommand(int Id) : IRequest<bool>;
