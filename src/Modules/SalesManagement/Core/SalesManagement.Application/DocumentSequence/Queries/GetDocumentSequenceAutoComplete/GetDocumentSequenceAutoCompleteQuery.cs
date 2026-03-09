using MediatR;
using SalesManagement.Application.DocumentSequence.Dto;

namespace SalesManagement.Application.DocumentSequence.Queries.GetDocumentSequenceAutoComplete
{
    public sealed record GetDocumentSequenceAutoCompleteQuery(string? Term)
        : IRequest<IReadOnlyList<DocumentSequenceLookupDto>>;
}
