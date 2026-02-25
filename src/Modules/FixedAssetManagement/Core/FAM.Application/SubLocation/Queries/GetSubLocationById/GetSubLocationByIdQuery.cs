using FAM.Application.SubLocation.Queries.GetSubLocations;
using MediatR;

namespace FAM.Application.SubLocation.Queries.GetSubLocationById
{
    public class GetSubLocationByIdQuery : IRequest<SubLocationDto>
    {
        public int Id { get; set; }
        
    }
}