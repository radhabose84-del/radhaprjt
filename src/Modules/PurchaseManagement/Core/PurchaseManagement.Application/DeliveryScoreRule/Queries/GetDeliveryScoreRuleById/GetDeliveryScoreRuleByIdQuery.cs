using MediatR;
using PurchaseManagement.Application.DeliveryScoreRule.Dto;

namespace PurchaseManagement.Application.DeliveryScoreRule.Queries.GetDeliveryScoreRuleById
{
    public class GetDeliveryScoreRuleByIdQuery : IRequest<DeliveryScoreRuleDto?>
    {
        public int Id { get; set; }
    }
}
