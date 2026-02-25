using MediatR;

namespace PurchaseManagement.Application.PurchaseIndents.Queries.GetPendingIndentById
{
    public class GetPendingIndentByIdQuery : IRequest<PendingIndentByIdDto>
    {
        public int Id { get; set; }
    }
}