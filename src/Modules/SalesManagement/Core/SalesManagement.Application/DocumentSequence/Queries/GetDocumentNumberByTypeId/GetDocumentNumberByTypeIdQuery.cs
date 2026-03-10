using MediatR;

namespace SalesManagement.Application.DocumentSequence.Queries.GetDocumentNumberByTypeId
{
    public class GetDocumentQuery : IRequest<IReadOnlyList<string>>
    {
        public int TypeId { get; set; }
    }
}
