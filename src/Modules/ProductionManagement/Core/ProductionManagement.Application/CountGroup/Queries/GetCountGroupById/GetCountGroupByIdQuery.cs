using MediatR;
using ProductionManagement.Application.CountGroup.Dto;

namespace ProductionManagement.Application.CountGroup.Queries.GetCountGroupById
{
    public class GetCountGroupByIdQuery : IRequest<CountGroupDto>
    {
        public int Id { get; set; }
    }
}
