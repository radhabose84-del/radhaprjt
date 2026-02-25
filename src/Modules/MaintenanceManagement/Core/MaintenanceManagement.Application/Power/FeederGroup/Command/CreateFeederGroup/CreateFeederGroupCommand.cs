using MediatR;

namespace MaintenanceManagement.Application.Power.FeederGroup.Command.CreateFeederGroup
{
    public class CreateFeederGroupCommand : IRequest<int>
    {
        public string? FeederGroupCode { get; set; }
        public string? FeederGroupName { get; set; }
        public int UnitId { get; set; }
       
    }
}