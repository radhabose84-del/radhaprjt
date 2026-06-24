using AutoMapper;
using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IJournal;
using FinanceManagement.Application.JournalMaster.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.JournalMaster.Journal.Queries.GetJournalAutoComplete
{
    public class GetJournalAutoCompleteQueryHandler : IRequestHandler<GetJournalAutoCompleteQuery, IReadOnlyList<JournalLookupDto>>
    {
        private readonly IJournalQueryRepository _queryRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetJournalAutoCompleteQueryHandler(IJournalQueryRepository queryRepository, IIPAddressService ipAddressService, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _ipAddressService = ipAddressService;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<JournalLookupDto>> Handle(GetJournalAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var companyId = _ipAddressService.GetCompanyId();
            var result = await _queryRepository.AutocompleteAsync(request.Term, companyId, request.StatusId, cancellationToken);
            var dtos = _mapper.Map<List<JournalLookupDto>>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetJournalAutoCompleteQuery",
                actionName: dtos.Count.ToString(),
                details: "Journal voucher autocomplete was fetched.",
                module: "Journal"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dtos;
        }
    }
}
