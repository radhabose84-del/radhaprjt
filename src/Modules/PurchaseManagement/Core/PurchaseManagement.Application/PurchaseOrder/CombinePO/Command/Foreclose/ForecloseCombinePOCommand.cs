using MediatR;

namespace PurchaseManagement.Application.PurchaseOrder.CombinePO.Command.Foreclose;

public sealed record ForecloseCombinePOCommand(int Id, int POMethodId) : IRequest<bool>;
