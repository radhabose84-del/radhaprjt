using AutoMapper;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IAccountingPeriod;
using FinanceManagement.Application.JournalMaster.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.JournalMaster.AccountingPeriod.Queries.GetAccountingPeriodById
{
    public class GetAccountingPeriodByIdQueryHandler : IRequestHandler<GetAccountingPeriodByIdQuery, AccountingPeriodDto?>
    {
        private readonly IAccountingPeriodQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAccountingPeriodByIdQueryHandler(IAccountingPeriodQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<AccountingPeriodDto?> Handle(GetAccountingPeriodByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.Id);

            if (result == null)
                return null;

            var dto = _mapper.Map<AccountingPeriodDto>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetAccountingPeriodByIdQuery",
                actionName: dto.Id.ToString(),
                details: $"Accounting Period details {dto.Id} was fetched.",
                module: "AccountingPeriod"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dto;
        }
    }
}
