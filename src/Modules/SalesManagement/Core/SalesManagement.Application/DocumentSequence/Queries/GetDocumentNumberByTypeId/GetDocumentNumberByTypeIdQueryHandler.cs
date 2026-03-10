using MediatR;
using SalesManagement.Application.Common.Interfaces.IDocumentSequence;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.DocumentSequence.Queries.GetDocumentNumberByTransactionTypeId
{
    public class GetDocumentQueryHandler : IRequestHandler<GetDocumentQuery, IReadOnlyList<string>>
    {
        private readonly IDocumentSequenceQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetDocumentQueryHandler(
            IDocumentSequenceQueryRepository queryRepository,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<string>> Handle(GetDocumentQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GenerateDocumentNumber(request.TransactionTypeId);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetDocument",
                actionCode: "GetDocumentQuery",
                actionName: request.TransactionTypeId.ToString(),
                details: $"Document Sequence generated numbers for TransactionTypeId '{request.TransactionTypeId}' were fetched.",
                module: "DocumentSequence"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
