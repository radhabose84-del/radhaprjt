using MediatR;

namespace MaintenanceManagement.Application.CostCenter.Command.DeleteCostCenter
{
    public class DeleteCostCenterCommand : IRequest<int> 
    {
        public int Id { get; set; }
    }
}