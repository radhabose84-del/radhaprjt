using AutoMapper;
using FinanceManagement.Application.Common.Interfaces.ITaxCode;
using FinanceManagement.Application.TaxCode.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.TaxCode.Queries.GetTaxAccountLinkageById
{
    public class GetTaxAccountLinkageByIdQueryHandler : IRequestHandler<GetTaxAccountLinkageByIdQuery, TaxAccountLinkageDto?>
    {
        private readonly ITaxCodeQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetTaxAccountLinkageByIdQueryHandler(ITaxCodeQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<TaxAccountLinkageDto?> Handle(GetTaxAccountLinkageByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetLinkageByIdAsync(request.Id);

            if (result == null)
                return null;

            var dto = _mapper.Map<TaxAccountLinkageDto>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetTaxAccountLinkageByIdQuery",
                actionName: dto.Id.ToString(),
                details: $"TaxAccountLinkage details {dto.Id} was fetched.",
                module: "TaxAccountLinkage"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dto;
        }
    }
}
