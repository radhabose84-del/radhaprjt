using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IGlAccountMaster;
using FinanceManagement.Application.GlAccountMaster.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.GlAccountMaster.Queries.GetGlAccountMasterAutoComplete
{
    public class GetGlAccountMasterAutoCompleteQueryHandler : IRequestHandler<GetGlAccountMasterAutoCompleteQuery, IReadOnlyList<GlAccountMasterLookupDto>>
    {
        private readonly IGlAccountMasterQueryRepository _queryRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetGlAccountMasterAutoCompleteQueryHandler(
            IGlAccountMasterQueryRepository queryRepository,
            IIPAddressService ipAddressService,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _ipAddressService = ipAddressService;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<GlAccountMasterLookupDto>> Handle(GetGlAccountMasterAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var companyId = _ipAddressService.GetCompanyId()
                ?? throw new ExceptionRules("No active company in session.");

            var result = await _queryRepository.AutocompleteAsync(request.Term, companyId, request.AccountTypeCode, cancellationToken);
            var dtos = _mapper.Map<List<GlAccountMasterLookupDto>>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetGlAccountMasterAutoCompleteQuery",
                actionName: dtos.Count.ToString(),
                details: "GlAccountMaster details was fetched.",
                module: "GlAccountMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dtos;
        }
    }
}
