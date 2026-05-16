using MediatR;

namespace PurchaseManagement.Application.PurchaseOrder.Local.Commands.Foreclose;

public sealed record ForeclosePurchaseOrderCommand(int Id) : IRequest<bool>;
