using MediatR;

namespace SalesManagement.Application.DocumentSequence.Queries.GetDocumentNumberByTransactionTypeId
{
    public class GetDocumentQuery : IRequest<IReadOnlyList<string>>
    {
        public int TransactionTypeId { get; set; }
    }
}
