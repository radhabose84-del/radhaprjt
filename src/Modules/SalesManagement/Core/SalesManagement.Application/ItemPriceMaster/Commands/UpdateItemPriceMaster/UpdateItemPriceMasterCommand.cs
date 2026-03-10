using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.ItemPriceMaster.Commands.UpdateItemPriceMaster
{
    public class UpdateItemPriceMasterCommand : IRequest<ApiResponseDTO<int>>
    {
        public int Id { get; set; }
        public int ItemId { get; set; }
        public int SalesSegmentId { get; set; }
        public int PaymentTermsId { get; set; }
        public decimal BaseRate { get; set; }
        public decimal ExMillRate { get; set; }
        public int CurrencyId { get; set; }
        public DateOnly ValidFrom { get; set; }
        public DateOnly ValidTo { get; set; }
        public int? StatusId { get; set; }
        public int IsActive { get; set; }
    }
}
