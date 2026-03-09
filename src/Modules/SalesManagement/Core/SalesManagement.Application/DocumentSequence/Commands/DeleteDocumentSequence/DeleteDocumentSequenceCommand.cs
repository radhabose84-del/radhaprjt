using MediatR;

namespace SalesManagement.Application.DocumentSequence.Commands.DeleteDocumentSequence
{
    public sealed record DeleteDocumentSequenceCommand(int Id) : IRequest<bool>;
}
