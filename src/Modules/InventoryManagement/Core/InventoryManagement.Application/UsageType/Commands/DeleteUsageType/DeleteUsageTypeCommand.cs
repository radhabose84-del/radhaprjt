using MediatR;

namespace InventoryManagement.Application.UsageType.Commands.DeleteUsageType
{
    public sealed record DeleteUsageTypeCommand(int Id) : IRequest<bool>;
}
