using MediatR;

namespace MaintenanceManagement.Application.Power.Feeder.Queries.GetFeederById
{
    public class GetFeederByIdQuery : IRequest<GetFeederByIdDto>
    {
        public int  Id { get; set; }        

    }
}