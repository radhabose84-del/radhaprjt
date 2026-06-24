using Contracts.Common;
using FinanceManagement.Application.JournalMaster.Dto;
using MediatR;

namespace FinanceManagement.Application.JournalMaster.RecurringJournalTemplate.Commands.UpdateRecurringJournalTemplate
{
    public class UpdateRecurringJournalTemplateCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public int Id { get; set; }
        public string? TemplateName { get; set; }
        public int VoucherTypeId { get; set; }
        public int FrequencyId { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public bool AutoPost { get; set; }
        public int AmountAdjustmentRuleId { get; set; }
        public bool LowRisk { get; set; }
        public int IsActive { get; set; }   // 1 = Active, 0 = Inactive
        public List<RecurringTemplateLineInputDto> Lines { get; set; } = new();

        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
