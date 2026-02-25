using MediatR;

namespace PurchaseManagement.Application.PurchaseIndents.Queries.GetPurchaseIndentById
{
    public class GetPurchaseIndentByIdQuery : IRequest<IndentByIdDto>
    {
        public int Id { get; set; }
        public int? SourceId { get; set; }
    }
}