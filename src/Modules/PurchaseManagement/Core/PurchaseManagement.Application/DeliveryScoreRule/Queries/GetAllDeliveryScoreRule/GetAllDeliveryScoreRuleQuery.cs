using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.DeliveryScoreRule.Dto;

namespace PurchaseManagement.Application.DeliveryScoreRule.Queries.GetAllDeliveryScoreRule
{
    public class GetAllDeliveryScoreRuleQuery : IRequest<ApiResponseDTO<List<DeliveryScoreRuleDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
    }
}
