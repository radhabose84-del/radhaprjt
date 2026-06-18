using Contracts.Common;
using FinanceManagement.Application.ScheduleIII.Dto;
using MediatR;

namespace FinanceManagement.Application.ScheduleIII.Commands.UpdateSubTotal
{
    // "Edit formula" / rename a sub-total node. Replaces the operand set and rebuilds the display expression.
    public class UpdateSubTotalCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public int Id { get; set; }
        public string? FormulaName { get; set; }   // "Gross Profit" / "EBITDA" / "Profit Before Tax" / "Profit After Tax"
        public bool IncludeOtherIncome { get; set; }
        public List<SubTotalFormulaInput> Formulas { get; set; } = new();

        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
