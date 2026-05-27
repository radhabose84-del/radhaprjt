using MediatR;

namespace PurchaseManagement.Application.PurchaseReturn.ReturnReason.Commands.DeleteReturnReason;

public sealed record DeleteReturnReasonCommand(int Id) : IRequest<bool>;
