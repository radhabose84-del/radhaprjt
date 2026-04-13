
using InventoryManagement.Application.ItemSpecificationMaster.Dto;
using MediatR;

namespace InventoryManagement.Application.ItemSpecificationMaster.Queries.GetItemSpecificationMasterById
{
    public class GetItemSpecificationMasterByIdQuery : IRequest<ItemSpecificationMasterDto?>
    {
        public int Id { get; set; }
    }
}
