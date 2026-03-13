using AutoMapper;
using FinanceManagement.Application.Common.Interfaces.ITransactionTypeMaster;
using FinanceManagement.Application.TransactionTypeMaster.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.TransactionTypeMaster.Queries.GetTransactionTypeMasterById
{
    public class GetTransactionTypeMasterByIdQueryHandler : IRequestHandler<GetTransactionTypeMasterByIdQuery, TransactionTypeMasterDto?>
    {
        private readonly ITransactionTypeMasterQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetTransactionTypeMasterByIdQueryHandler(
            ITransactionTypeMasterQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<TransactionTypeMasterDto?> Handle(GetTransactionTypeMasterByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.Id);

            if (result == null)
                return null;

            var dto = _mapper.Map<TransactionTypeMasterDto>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetTransactionTypeMasterByIdQuery",
                actionName: dto.Id.ToString(),
                details: $"Transaction Type Master details {dto.Id} was fetched.",
                module: "TransactionTypeMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dto;
        }
    }
}
