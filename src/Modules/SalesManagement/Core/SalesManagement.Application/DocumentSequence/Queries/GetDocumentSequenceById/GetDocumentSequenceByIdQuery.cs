using MediatR;
using SalesManagement.Application.DocumentSequence.Dto;

namespace SalesManagement.Application.DocumentSequence.Queries.GetDocumentSequenceById
{
    public class GetDocumentSequenceByIdQuery : IRequest<DocumentSequenceDto?>
    {
        public int Id { get; set; }
    }
}
