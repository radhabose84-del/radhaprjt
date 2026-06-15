using MediatR;
using Contracts.Common;

namespace MaintenanceManagement.Application.Power.GeneratorConsumption.Command
{
    public class CreateGeneratorConsumptionCommand : IRequest<int>, IRequirePermission
    {
        public int GeneratorId { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
        public decimal DieselConsumption { get; set; }
        public decimal OpeningEnergyReading { get; set; }
        public decimal ClosingEnergyReading { get; set; }
        public int? PurposeId { get; set; }  
        public int UnitId { get; set; }

        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
