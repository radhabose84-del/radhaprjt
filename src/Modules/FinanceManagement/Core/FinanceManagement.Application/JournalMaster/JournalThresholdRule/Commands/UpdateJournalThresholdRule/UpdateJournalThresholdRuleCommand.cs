using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.JournalMaster.JournalThresholdRule.Commands.UpdateJournalThresholdRule
{
    public class UpdateJournalThresholdRuleCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public int Id { get; set; }
        public int RuleTypeId { get; set; }
        public decimal? ThresholdValue { get; set; }
        public bool Active { get; set; }
        public DateOnly EffectiveFrom { get; set; }
        public int IsActive { get; set; }   // 1 = Active, 0 = Inactive

        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
