using Contracts.Common;
using MediatR;

namespace PurchaseManagement.Application.DeliveryScoreRule.Commands.UpdateDeliveryScoreRule
{
    public class UpdateDeliveryScoreRuleCommand : IRequest<ApiResponseDTO<int>>
    {
        public int Id { get; set; }
        public string? Description { get; set; }
        public int DelayDaysFrom { get; set; }
        public int DelayDaysTo { get; set; }
        public decimal Score { get; set; }
        public int SortOrder { get; set; }
        public int IsActive { get; set; }
    }
}
