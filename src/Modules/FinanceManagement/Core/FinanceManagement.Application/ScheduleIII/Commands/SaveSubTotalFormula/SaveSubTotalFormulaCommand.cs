using Contracts.Common;
using FinanceManagement.Application.ScheduleIII.Dto;
using MediatR;

namespace FinanceManagement.Application.ScheduleIII.Commands.SaveSubTotalFormula
{
    // "Edit formula" Save — replaces the operand set of one sub-total. Old rows are physically
    // deleted (logged to Finance.ActivityLog) and the new selection is inserted.
    public class SaveSubTotalFormulaCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public int SubTotalId { get; set; }
        public List<SubTotalFormulaInput> Formulas { get; set; } = new();

        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
