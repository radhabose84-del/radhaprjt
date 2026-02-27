using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.SalesItemPriceMaster.Commands.CreateSalesItemPriceMaster
{
    public class CreateSalesItemPriceMasterCommand : IRequest<ApiResponseDTO<int>>
    {
        public string? PriceCode { get; set; }
        public int ItemId { get; set; }
        public int SalesSegmentId { get; set; }
        public int PaymentTermsId { get; set; }
        public decimal ExMillPrice { get; set; }
        public int CurrencyId { get; set; }
        public DateTimeOffset ValidFrom { get; set; }
        public DateTimeOffset ValidTo { get; set; }
    }
}
