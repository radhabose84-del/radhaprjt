using FAM.Application.Location.Queries.GetLocations;
using MediatR;

namespace FAM.Application.Location.Queries.GetLocationById
{
    public class GetLocationByIdQuery : IRequest<LocationDto>
    {
        public int Id { get; set; }
        
    }
}