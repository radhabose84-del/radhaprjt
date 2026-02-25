using FAM.Application.UOM.Queries.GetUOMs;
using MediatR;

namespace FAM.Application.UOM.Queries.GetUOMById
{
    public class GetUOMByIdQuery : IRequest<UOMDto>
    {
        public int Id { get; set; }
        
    }
}