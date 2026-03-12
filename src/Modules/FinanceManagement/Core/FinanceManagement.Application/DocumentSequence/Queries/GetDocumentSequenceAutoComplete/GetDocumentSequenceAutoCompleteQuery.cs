using FinanceManagement.Application.DocumentSequence.Dto;
using MediatR;

namespace FinanceManagement.Application.DocumentSequence.Queries.GetDocumentSequenceAutoComplete
{
    public sealed record GetDocumentSequenceAutoCompleteQuery(string? Term)
        : IRequest<IReadOnlyList<DocumentSequenceLookupDto>>;
}
