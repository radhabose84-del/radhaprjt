using MediatR;

namespace PurchaseManagement.Application.PurchaseOrder.ImportPO.Command.Cancel;

public sealed record CancelImportPOCommand(int Id) : IRequest<bool>;
