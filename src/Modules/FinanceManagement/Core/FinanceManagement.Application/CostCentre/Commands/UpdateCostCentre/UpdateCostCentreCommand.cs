using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.CostCentre.Commands.UpdateCostCentre
{
    public class UpdateCostCentreCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public int Id { get; set; }
        // CostCentreCode, CentreLevelId, ParentCostCentreId, UnitId (Plant) are immutable — not included.
        public string? CostCentreName { get; set; }
        public int IsActive { get; set; }   // 1 = Active, 0 = Inactive

        // Optional (non-mandatory) — editable when the FE adds the inputs.
        public int? ResponsibleManagerId { get; set; }
        public DateTimeOffset? EffectiveFromDate { get; set; }
        public DateTimeOffset? EffectiveToDate { get; set; }

        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
