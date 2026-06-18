using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.ScheduleIII.Commands.UpdateSubTotal
{
    // Sub-total HEADER only (rename / flags / order / active). Operands → ScheduleIIISubTotalFormula API.
    public class UpdateSubTotalCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public int Id { get; set; }
        public string FormulaName { get; set; } = string.Empty;   // "Gross Profit" / "EBITDA" / "Profit Before Tax" / "Profit After Tax"
        public bool IncludeOtherIncome { get; set; }
        public int DisplayOrder { get; set; }
        public int IsActive { get; set; }   // 1 = Active, 0 = Inactive

        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
