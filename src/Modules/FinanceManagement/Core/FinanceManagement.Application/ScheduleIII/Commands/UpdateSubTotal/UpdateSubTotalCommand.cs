using Contracts.Common;
using FinanceManagement.Application.ScheduleIII.Dto;
using MediatR;

namespace FinanceManagement.Application.ScheduleIII.Commands.UpdateSubTotal
{
    // Rename / edit a sub-total header and replace its operand set (old operands hard-deleted, logged to ActivityLog).
    public class UpdateSubTotalCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public int Id { get; set; }
        public string FormulaName { get; set; } = string.Empty;   // "Gross Profit" / "EBITDA" / "Profit Before Tax" / "Profit After Tax"
        public bool IncludeOtherIncome { get; set; }
        public int DisplayOrder { get; set; }
        public int IsActive { get; set; }   // 1 = Active, 0 = Inactive
        public List<SubTotalFormulaInput> Formulas { get; set; } = new();

        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
