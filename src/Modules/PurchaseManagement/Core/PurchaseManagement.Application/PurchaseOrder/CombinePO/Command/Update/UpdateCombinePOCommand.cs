using MediatR;

namespace PurchaseManagement.Application.PurchaseOrder.CombinePO.Commands.Update;

public sealed record UpdateCombinePOCommand(UpdateCombinePODto Data) : IRequest<bool>;
