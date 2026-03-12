using FinanceManagement.Application.DocumentSequence.Dto;
using MediatR;

namespace FinanceManagement.Application.DocumentSequence.Queries.GetDocumentSequenceById
{
    public class GetDocumentSequenceByIdQuery : IRequest<DocumentSequenceDto?>
    {
        public int Id { get; set; }
    }
}
