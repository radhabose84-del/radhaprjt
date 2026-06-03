using MediatR;

namespace UserManagement.Application.Location.Queries.GetLocationById
{
    public class GetLocationByIdQuery : IRequest<LocationByIdDto>
    {
        public int Id { get; set; }
    }
}
