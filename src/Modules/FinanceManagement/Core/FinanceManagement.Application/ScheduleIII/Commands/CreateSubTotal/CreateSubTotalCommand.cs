using Contracts.Common;
using FinanceManagement.Application.ScheduleIII.Dto;
using MediatR;

namespace FinanceManagement.Application.ScheduleIII.Commands.CreateSubTotal
{
    public class CreateSubTotalCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public int StructureId { get; set; }
        public string? SubTotalName { get; set; }
        public bool IncludeOtherIncome { get; set; }
        public int DisplayOrder { get; set; }
        public List<SubTotalFormulaInput> Formulas { get; set; } = new();

        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
