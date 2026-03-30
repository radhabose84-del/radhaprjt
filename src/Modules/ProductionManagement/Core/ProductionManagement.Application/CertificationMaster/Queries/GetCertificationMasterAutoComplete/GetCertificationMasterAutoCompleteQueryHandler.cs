using AutoMapper;
using Contracts.Dtos.Lookups.Production;
using MediatR;
using ProductionManagement.Application.Common.Interfaces.ICertificationMaster;
using ProductionManagement.Domain.Events;

namespace ProductionManagement.Application.CertificationMaster.Queries.GetCertificationMasterAutoComplete
{
    public class GetCertificationMasterAutoCompleteQueryHandler : IRequestHandler<GetCertificationMasterAutoCompleteQuery, IReadOnlyList<CertificationMasterLookupDto>>
    {
        private readonly ICertificationMasterQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetCertificationMasterAutoCompleteQueryHandler(
            ICertificationMasterQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<CertificationMasterLookupDto>> Handle(GetCertificationMasterAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.AutocompleteAsync(request.Term ?? string.Empty, cancellationToken);
            var dtos = _mapper.Map<List<CertificationMasterLookupDto>>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetCertificationMasterAutoCompleteQuery",
                actionName: dtos.Count.ToString(),
                details: "Certification Master details was fetched.",
                module: "CertificationMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dtos;
        }
    }
}
