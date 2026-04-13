
using InventoryManagement.Application.ItemSpecificationValue.Dto;
using MediatR;

namespace InventoryManagement.Application.ItemSpecificationValue.Queries.GetItemSpecificationValueById
{
    public class GetItemSpecificationValueByIdQuery : IRequest<ItemSpecificationValueDto?>
    {
        public int Id { get; set; }
    }
}
