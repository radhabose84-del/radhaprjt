using MediatR;
using SalesManagement.Application.Common.Interfaces.IDocumentSequence;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.DocumentSequence.Commands.DeleteDocumentSequence
{
    public class DeleteDocumentSequenceCommandHandler : IRequestHandler<DeleteDocumentSequenceCommand, bool>
    {
        private readonly IDocumentSequenceCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public DeleteDocumentSequenceCommandHandler(
            IDocumentSequenceCommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteDocumentSequenceCommand request, CancellationToken cancellationToken)
        {
            await _commandRepository.SoftDeleteAsync(request.Id, cancellationToken);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "DOCUMENT_SEQUENCE_DELETE",
                actionName: request.Id.ToString(),
                details: $"Document Sequence with Id {request.Id} soft deleted.",
                module: "DocumentSequence"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
