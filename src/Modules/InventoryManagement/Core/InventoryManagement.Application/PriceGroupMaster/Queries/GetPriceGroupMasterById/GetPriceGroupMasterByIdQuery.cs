using InventoryManagement.Application.PriceGroupMaster.Dto;
using MediatR;

namespace InventoryManagement.Application.PriceGroupMaster.Queries.GetPriceGroupMasterById
{
    public class GetPriceGroupMasterByIdQuery : IRequest<PriceGroupMasterDto?>
    {
        public int Id { get; set; }
    }
}
