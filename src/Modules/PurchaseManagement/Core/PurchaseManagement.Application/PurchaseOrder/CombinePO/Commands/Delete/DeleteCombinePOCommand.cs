using MediatR;

namespace PurchaseManagement.Application.PurchaseOrder.CombinePO.Commands.Delete;

public sealed record DeleteCombinePOCommand(int Id, int POMethodId) : IRequest<bool>;
