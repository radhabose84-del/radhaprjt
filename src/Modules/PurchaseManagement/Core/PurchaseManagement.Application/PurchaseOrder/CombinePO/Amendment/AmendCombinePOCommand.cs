using MediatR;

namespace PurchaseManagement.Application.PurchaseOrder.CombinePO.Amendment;

public sealed record AmendCombinePOCommand(AmendCombinePODto Data) : IRequest<int>;
