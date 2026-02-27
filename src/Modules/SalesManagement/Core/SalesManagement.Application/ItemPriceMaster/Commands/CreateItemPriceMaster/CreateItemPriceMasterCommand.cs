using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.ItemPriceMaster.Commands.CreateItemPriceMaster
{
    public class CreateItemPriceMasterCommand : IRequest<ApiResponseDTO<int>>
    {
        public string? PriceCode { get; set; }
        public int ItemId { get; set; }
        public int SalesSegmentId { get; set; }
        public int PaymentTermsId { get; set; }
        public decimal ExMillPrice { get; set; }
        public int CurrencyId { get; set; }
        public DateOnly ValidFrom { get; set; }
        public DateOnly ValidTo { get; set; }
    }
}
