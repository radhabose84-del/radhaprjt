using MediatR;

namespace MaintenanceManagement.Application.Power.FeederGroup.Queries.GetFeederGroupById
{
    public class GetFeederGroupByIdQuery :IRequest<GetFeederGroupByIdDto>
    {

     public int  Id { get; set; }
        
    }
}