using Contracts.Common;
using MediatR;
using SalesManagement.Application.ItemPriceMaster.Dto;

namespace SalesManagement.Application.ItemPriceMaster.Queries.GetItemPriceByItemAndDate
{
    public class GetItemPriceByItemAndDateQuery : IRequest<ApiResponseDTO<List<ItemPriceMasterDto>>>
    {
        public int ItemId { get; set; }
        public DateOnly Date { get; set; }
    }
}
