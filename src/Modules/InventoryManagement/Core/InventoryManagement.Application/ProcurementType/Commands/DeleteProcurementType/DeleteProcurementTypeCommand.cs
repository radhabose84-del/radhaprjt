using MediatR;

namespace InventoryManagement.Application.ProcurementType.Commands.DeleteProcurementType
{
    public sealed record DeleteProcurementTypeCommand(int Id) : IRequest<bool>;
}
