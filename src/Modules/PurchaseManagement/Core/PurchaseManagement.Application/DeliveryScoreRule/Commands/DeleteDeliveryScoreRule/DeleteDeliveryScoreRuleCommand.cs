using MediatR;

namespace PurchaseManagement.Application.DeliveryScoreRule.Commands.DeleteDeliveryScoreRule;

public sealed record DeleteDeliveryScoreRuleCommand(int Id) : IRequest<bool>;
