using MediatR;
using SalesManagement.Application.DocumentSequence.Dto;

namespace SalesManagement.Application.DocumentSequence.Queries.GetDocumentNumberByTypeId
{
    public class GetDocumentNumberByTypeIdQuery : IRequest<IReadOnlyList<DocumentSequenceGeneratedDto>>
    {
        public int TypeId { get; set; }
    }
}
