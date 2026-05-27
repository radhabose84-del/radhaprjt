using Contracts.Common;
using MediatR;

namespace PurchaseManagement.Application.DeliveryScoreRule.Commands.CreateDeliveryScoreRule
{
    public class CreateDeliveryScoreRuleCommand : IRequest<ApiResponseDTO<int>>
    {
        public string? RuleCode { get; set; }
        public string? Description { get; set; }
        public int DelayDaysFrom { get; set; }
        public int DelayDaysTo { get; set; }
        public decimal Score { get; set; }
        public int SortOrder { get; set; }
    }
}
