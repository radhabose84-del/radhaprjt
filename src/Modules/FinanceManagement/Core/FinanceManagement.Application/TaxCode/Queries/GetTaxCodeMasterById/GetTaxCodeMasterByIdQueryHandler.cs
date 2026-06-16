using AutoMapper;
using FinanceManagement.Application.Common.Interfaces.ITaxCode;
using FinanceManagement.Application.TaxCode.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.TaxCode.Queries.GetTaxCodeMasterById
{
    public class GetTaxCodeMasterByIdQueryHandler : IRequestHandler<GetTaxCodeMasterByIdQuery, TaxCodeMasterDto?>
    {
        private readonly ITaxCodeQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetTaxCodeMasterByIdQueryHandler(ITaxCodeQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<TaxCodeMasterDto?> Handle(GetTaxCodeMasterByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetTaxCodeByIdAsync(request.Id);

            if (result == null)
                return null;

            var dto = _mapper.Map<TaxCodeMasterDto>(result);
            dto.RateVersions = result.RateVersions;   // carry merged rate history through the mapping

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetTaxCodeMasterByIdQuery",
                actionName: dto.Id.ToString(),
                details: $"TaxCodeMaster details {dto.Id} was fetched.",
                module: "TaxCodeMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dto;
        }
    }
}
