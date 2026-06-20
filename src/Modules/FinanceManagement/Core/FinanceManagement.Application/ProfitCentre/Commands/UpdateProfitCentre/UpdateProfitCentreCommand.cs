using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.ProfitCentre.Commands.UpdateProfitCentre
{
    public class UpdateProfitCentreCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public int Id { get; set; }
        // ProfitCentreCode, LevelId, ParentProfitCentreId are immutable — not included.
        public string? ProfitCentreName { get; set; }
        public int IsActive { get; set; }   // 1 = Active, 0 = Inactive

        public bool IsRevenueLinked { get; set; } = true;

        // Optional — editable when the FE wires the picker.
        public int? ResponsibleHeadId { get; set; }

        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
