using MediatR;
using PurchaseManagement.Application.PurchaseOrder.Dtos.BlanketPO;

namespace PurchaseManagement.Application.PurchaseOrder.BlanketPO.Commands.Update;

public sealed record UpdateBlanketPOCommand(BlanketPOUpdateDto Data)
    : IRequest<bool>;
