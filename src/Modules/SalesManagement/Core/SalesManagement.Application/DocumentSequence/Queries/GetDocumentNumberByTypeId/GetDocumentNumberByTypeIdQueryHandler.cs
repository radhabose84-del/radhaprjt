using MediatR;
using SalesManagement.Application.Common.Interfaces.IDocumentSequence;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.DocumentSequence.Queries.GetDocumentNumberByTypeId
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
            var result = await _queryRepository.GenerateDocumentNumber(request.TypeId);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetDocument",
                actionCode: "GetDocumentQuery",
                actionName: request.TypeId.ToString(),
                details: $"Document Sequence generated numbers for TypeId '{request.TypeId}' were fetched.",
                module: "DocumentSequence"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
