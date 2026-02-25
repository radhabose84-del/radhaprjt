using MaintenanceManagement.Application.Power.PowerConsumption.Queries.GetFeederSubFeederById;
using MediatR;

namespace MaintenanceManagement.Application.Power.PowerConsumption.Queries
{
    public class GetFeederSubFeederByIdQuery :  IRequest<List<GetFeederSubFeederDto>>
    {
        public int FeederTypeId { get; set; }
    }
}