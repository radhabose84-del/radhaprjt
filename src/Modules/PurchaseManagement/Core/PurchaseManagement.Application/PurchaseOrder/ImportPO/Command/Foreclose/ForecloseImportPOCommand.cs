using MediatR;

namespace PurchaseManagement.Application.PurchaseOrder.ImportPO.Command.Foreclose;

public sealed record ForecloseImportPOCommand(int Id) : IRequest<bool>;
