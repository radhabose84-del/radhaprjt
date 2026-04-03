using MediatR;
using ProductionManagement.Application.RepackingHeader.Dto;

namespace ProductionManagement.Application.RepackingHeader.Queries.GetRepackingHeaderById
{
    public class GetRepackingHeaderByIdQuery : IRequest<RepackingHeaderDto?>
    {
        public int Id { get; set; }
    }
}
