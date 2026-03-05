using MediatR;
using SalesManagement.Application.StoHeader.Dto;

namespace SalesManagement.Application.StoHeader.Queries.GetStoHeaderById
{
    public class GetStoHeaderByIdQuery : IRequest<StoHeaderDto?>
    {
        public int Id { get; set; }
    }
}
