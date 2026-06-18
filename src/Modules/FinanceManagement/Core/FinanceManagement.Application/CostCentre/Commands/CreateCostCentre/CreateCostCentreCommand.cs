using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.CostCentre.Commands.CreateCostCentre
{
    public class CreateCostCentreCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public string? CostCentreCode { get; set; }   // unique per unit, immutable
        public string? CostCentreName { get; set; }

        public int CentreLevelId { get; set; }         // Finance.MiscMaster (L1 Plant / L2 Department Group / L3 Department)
        public int? ParentCostCentreId { get; set; }   // null for L1; L1 for L2; L2 for L3

        public int? DepartmentGroupId { get; set; }    // set for L2 & L3
        public int? DepartmentId { get; set; }         // set for L3 only

        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
