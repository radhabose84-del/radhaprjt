using AutoMapper;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IJournal;
using FinanceManagement.Application.JournalMaster.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.JournalMaster.Journal.Queries.GetJournalById
{
    public class GetJournalByIdQueryHandler : IRequestHandler<GetJournalByIdQuery, JournalHeaderDto?>
    {
        private readonly IJournalQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetJournalByIdQueryHandler(IJournalQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<JournalHeaderDto?> Handle(GetJournalByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.Id);

            if (result == null)
                return null;

            var dto = _mapper.Map<JournalHeaderDto>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetJournalByIdQuery",
                actionName: dto.Id.ToString(),
                details: $"Journal voucher details {dto.Id} was fetched.",
                module: "Journal"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dto;
        }
    }
}
