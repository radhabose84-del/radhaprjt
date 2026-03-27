using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.ItemPriceMaster.Commands.CreateItemPriceMaster
{
    public class CreateItemPriceMasterCommand : IRequest<ApiResponseDTO<int>>
    {
        public int ItemId { get; set; }
        public int SalesSegmentId { get; set; }
        public decimal BaseRate { get; set; }
        public int CurrencyId { get; set; }
        public DateOnly ValidFrom { get; set; }
        public DateOnly ValidTo { get; set; }
        public decimal? TolerancePercentage { get; set; }
        public int? StatusId { get; set; }
    }
}
