using MaintenanceManagement.Application.CostCenter.Queries.GetCostCenter;
using MediatR;

namespace MaintenanceManagement.Application.CostCenter.Queries.GetCostCenterById
{
    public class GetCostCenterByIdQuery : IRequest<CostCenterDto?>
    {
           public int Id { get; set; }
    }
}