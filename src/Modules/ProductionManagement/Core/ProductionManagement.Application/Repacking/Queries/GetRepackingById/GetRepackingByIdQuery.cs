using MediatR;
using ProductionManagement.Application.Repacking.Dto;

namespace ProductionManagement.Application.Repacking.Queries.GetRepackingById
{
    public class GetRepackingByIdQuery : IRequest<RepackingHeaderDto>
    {
        public int Id { get; set; }
    }
}
