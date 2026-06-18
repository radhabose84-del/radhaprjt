using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.ScheduleIII.Commands.CreateSubTotal
{
    // Sub-total HEADER only. Operands are managed separately via the ScheduleIIISubTotalFormula API.
    public class CreateSubTotalCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public string FormulaName { get; set; } = string.Empty;   // "Gross Profit" / "EBITDA" / "Profit Before Tax" / "Profit After Tax"
        public bool IncludeOtherIncome { get; set; }
        public int DisplayOrder { get; set; }

        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
