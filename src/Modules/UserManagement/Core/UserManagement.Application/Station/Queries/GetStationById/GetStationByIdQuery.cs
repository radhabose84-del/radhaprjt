using MediatR;

namespace UserManagement.Application.Station.Queries.GetStationById
{
    public class GetStationByIdQuery : IRequest<StationByIdDto>
    {
        public int Id { get; set; }
    }
}
