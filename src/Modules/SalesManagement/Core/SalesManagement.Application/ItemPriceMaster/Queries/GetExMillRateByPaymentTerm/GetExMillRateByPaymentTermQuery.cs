using Contracts.Common;
using MediatR;
using SalesManagement.Application.ItemPriceMaster.Dto;

namespace SalesManagement.Application.ItemPriceMaster.Queries.GetExMillRateByPaymentTerm
{
    public class GetExMillRateByPaymentTermQuery : IRequest<ApiResponseDTO<List<ExMillRateDto>>>
    {
        public int PaymentTermId { get; set; }
        public int? SalesSegmentId { get; set; }
        public int ItemId { get; set; }
    }
}
