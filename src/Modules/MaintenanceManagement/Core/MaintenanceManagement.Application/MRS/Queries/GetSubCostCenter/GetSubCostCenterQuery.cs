using MediatR;

namespace MaintenanceManagement.Application.MRS.Queries.GetSubCostCenter
{
    public class GetSubCostCenterQuery : IRequest<List<MSubCostCenterDto>>
    {
         public string? OldUnitcode { get; set; }
    }
}