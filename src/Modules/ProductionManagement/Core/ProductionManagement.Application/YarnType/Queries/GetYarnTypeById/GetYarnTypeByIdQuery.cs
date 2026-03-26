using MediatR;
using ProductionManagement.Application.YarnType.Dto;

namespace ProductionManagement.Application.YarnType.Queries.GetYarnTypeById
{
    public class GetYarnTypeByIdQuery : IRequest<YarnTypeDto>
    {
        public int Id { get; set; }
    }
}
