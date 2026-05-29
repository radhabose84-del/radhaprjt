using MediatR;

namespace PurchaseManagement.Application.PurchaseOrder.BlanketPO.Commands.Foreclose;

public sealed record ForecloseBlanketPOCommand(int Id) : IRequest<bool>;
