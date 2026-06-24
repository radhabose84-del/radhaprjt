using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.JournalMaster.JournalThresholdRule.Commands.CreateJournalThresholdRule
{
    public class CreateJournalThresholdRuleCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public int RuleTypeId { get; set; }
        public decimal? ThresholdValue { get; set; }
        public bool Active { get; set; }
        public DateOnly EffectiveFrom { get; set; }

        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
