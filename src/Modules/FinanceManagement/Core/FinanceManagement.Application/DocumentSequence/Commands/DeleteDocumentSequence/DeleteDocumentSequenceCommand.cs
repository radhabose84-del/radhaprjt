using MediatR;

namespace FinanceManagement.Application.DocumentSequence.Commands.DeleteDocumentSequence
{
    public sealed record DeleteDocumentSequenceCommand(int Id) : IRequest<bool>;
}
