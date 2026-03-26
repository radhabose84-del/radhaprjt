using MediatR;

namespace SalesManagement.Application.StoHeader.Queries.GetPendingStoHeaderById
{
    public class GetPendingStoHeaderByIdQuery : IRequest<PendingStoHeaderByIdDto?>
    {
        public int Id { get; set; }
    }
}
