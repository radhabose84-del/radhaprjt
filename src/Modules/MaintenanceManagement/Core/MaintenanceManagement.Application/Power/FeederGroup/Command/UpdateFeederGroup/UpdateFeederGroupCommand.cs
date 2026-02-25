using MediatR;

namespace MaintenanceManagement.Application.Power.FeederGroup.Command.UpdateFeederGroup
{
    public class UpdateFeederGroupCommand  : IRequest<bool>
    { 
        public int Id { get; set; }
        public string? FeederGroupCode { get; set; }
        public string? FeederGroupName { get; set; }  
        public int  UnitId { get; set; }      
        public byte IsActive { get; set; }
    }
}