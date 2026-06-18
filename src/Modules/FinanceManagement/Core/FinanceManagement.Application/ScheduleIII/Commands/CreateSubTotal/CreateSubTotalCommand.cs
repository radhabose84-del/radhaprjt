using Contracts.Common;
using FinanceManagement.Application.ScheduleIII.Dto;
using MediatR;

namespace FinanceManagement.Application.ScheduleIII.Commands.CreateSubTotal
{
    // Sub-total header + its signed operands, persisted together.
    public class CreateSubTotalCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public string FormulaName { get; set; } = string.Empty;   // "Gross Profit" / "EBITDA" / "Profit Before Tax" / "Profit After Tax"
        public bool IncludeOtherIncome { get; set; }
        public int DisplayOrder { get; set; }
        public List<SubTotalFormulaInput> Formulas { get; set; } = new();

        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
