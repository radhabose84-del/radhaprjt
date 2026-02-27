using MediatR;
using SalesManagement.Application.ItemPriceMaster.Dto;

namespace SalesManagement.Application.ItemPriceMaster.Queries.GetItemPriceMasterById
{
    public class GetItemPriceMasterByIdQuery : IRequest<ItemPriceMasterDto?>
    {
        public int Id { get; set; }
    }
}
