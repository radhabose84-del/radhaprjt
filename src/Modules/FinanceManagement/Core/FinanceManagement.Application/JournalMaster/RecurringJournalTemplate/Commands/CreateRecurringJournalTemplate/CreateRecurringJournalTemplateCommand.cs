using Contracts.Common;
using FinanceManagement.Application.JournalMaster.Dto;
using MediatR;

namespace FinanceManagement.Application.JournalMaster.RecurringJournalTemplate.Commands.CreateRecurringJournalTemplate
{
    public class CreateRecurringJournalTemplateCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public string? TemplateName { get; set; }
        public int VoucherTypeId { get; set; }
        public int FrequencyId { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public bool AutoPost { get; set; }
        public int AmountAdjustmentRuleId { get; set; }
        public bool LowRisk { get; set; }
        public List<RecurringTemplateLineInputDto> Lines { get; set; } = new();

        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
