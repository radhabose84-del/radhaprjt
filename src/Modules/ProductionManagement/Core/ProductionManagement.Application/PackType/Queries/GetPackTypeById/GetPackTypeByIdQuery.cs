using MediatR;
using ProductionManagement.Application.PackType.Dto;

namespace ProductionManagement.Application.PackType.Queries.GetPackTypeById
{
    public class GetPackTypeByIdQuery : IRequest<PackTypeDto?>
    {
        public int Id { get; set; }
    }
}
