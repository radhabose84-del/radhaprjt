using AutoMapper;
using FinanceManagement.Application.Common.Interfaces.ICurrencyForexConfig;
using FinanceManagement.Application.CurrencyForexConfig.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.CurrencyForexConfig.Queries.GetCurrencyForexConfigById
{
    public class GetCurrencyForexConfigByIdQueryHandler : IRequestHandler<GetCurrencyForexConfigByIdQuery, CurrencyForexConfigDto?>
    {
        private readonly ICurrencyForexConfigQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetCurrencyForexConfigByIdQueryHandler(ICurrencyForexConfigQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<CurrencyForexConfigDto?> Handle(GetCurrencyForexConfigByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.Id);

            if (result == null)
                return null;

            var dto = _mapper.Map<CurrencyForexConfigDto>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetCurrencyForexConfigByIdQuery",
                actionName: dto.Id.ToString(),
                details: $"CurrencyForexConfig details {dto.Id} was fetched.",
                module: "CurrencyForexConfig"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dto;
        }
    }
}
