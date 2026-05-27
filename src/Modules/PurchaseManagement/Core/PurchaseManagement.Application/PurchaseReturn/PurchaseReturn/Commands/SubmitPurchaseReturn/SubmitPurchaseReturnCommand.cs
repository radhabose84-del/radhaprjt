using MediatR;

namespace PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Commands.SubmitPurchaseReturn;

public sealed record SubmitPurchaseReturnCommand(int Id) : IRequest<bool>;
