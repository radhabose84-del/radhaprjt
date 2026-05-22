using MediatR;
using Contracts.Common;

namespace MaintenanceManagement.Application.Power.PowerConsumption.Command.CreatePowerConsumption
{
    public class CreatePowerConsumptionCommand : IRequest<int>, IRequirePermission
    {
        public int FeederTypeId { get; set; }
        public int FeederId { get; set; }
        public int UnitId { get; set; }
        public decimal OpeningReading { get; set; }
        public decimal ClosingReading { get; set; }
        
        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
