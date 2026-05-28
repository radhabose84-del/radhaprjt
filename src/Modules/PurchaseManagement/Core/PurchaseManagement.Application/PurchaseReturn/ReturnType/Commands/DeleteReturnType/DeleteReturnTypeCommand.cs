using MediatR;

namespace PurchaseManagement.Application.PurchaseReturn.ReturnType.Commands.DeleteReturnType;

public sealed record DeleteReturnTypeCommand(int Id) : IRequest<bool>;
