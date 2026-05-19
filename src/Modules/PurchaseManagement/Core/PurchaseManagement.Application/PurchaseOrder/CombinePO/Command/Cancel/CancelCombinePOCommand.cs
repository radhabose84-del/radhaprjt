using MediatR;

namespace PurchaseManagement.Application.PurchaseOrder.CombinePO.Command.Cancel;

public sealed record CancelCombinePOCommand(int Id, int POMethodId) : IRequest<bool>;
