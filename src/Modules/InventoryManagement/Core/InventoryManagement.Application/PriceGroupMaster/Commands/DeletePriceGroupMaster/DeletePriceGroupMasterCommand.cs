using MediatR;

namespace InventoryManagement.Application.PriceGroupMaster.Commands.DeletePriceGroupMaster
{
    public sealed record DeletePriceGroupMasterCommand(int Id) : IRequest<bool>;
}
