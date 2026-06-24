using FinanceManagement.Application.Common.Interfaces.JournalMaster.IJournalImport;
using FinanceManagement.Application.JournalMaster.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.JournalMaster.JournalImport.Queries.GetJournalImportBatchById
{
    public class GetJournalImportBatchByIdQueryHandler : IRequestHandler<GetJournalImportBatchByIdQuery, JournalImportBatchDto?>
    {
        private readonly IJournalImportQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetJournalImportBatchByIdQueryHandler(IJournalImportQueryRepository queryRepository, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<JournalImportBatchDto?> Handle(GetJournalImportBatchByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetBatchByIdAsync(request.Id);

            if (result == null)
                return null;

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetJournalImportBatchByIdQuery",
                actionName: result.Id.ToString(),
                details: $"Journal import batch {result.Id} was fetched.",
                module: "JournalImport"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
