using AutoMapper;
using FinanceManagement.Application.AccountTypeMaster.Dto;
using FinanceManagement.Application.Common.Interfaces.IAccountTypeMaster;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.AccountTypeMaster.Queries.GetAccountTypeMasterAutoComplete
{
    public class GetAccountTypeMasterAutoCompleteQueryHandler : IRequestHandler<GetAccountTypeMasterAutoCompleteQuery, IReadOnlyList<AccountTypeMasterLookupDto>>
    {
        private readonly IAccountTypeMasterQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAccountTypeMasterAutoCompleteQueryHandler(IAccountTypeMasterQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<AccountTypeMasterLookupDto>> Handle(GetAccountTypeMasterAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.AutocompleteAsync(request.Term, request.CompanyId, cancellationToken);
            var dtos = _mapper.Map<List<AccountTypeMasterLookupDto>>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetAccountTypeMasterAutoCompleteQuery",
                actionName: dtos.Count.ToString(),
                details: "AccountTypeMaster details was fetched.",
                module: "AccountTypeMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dtos;
        }
    }
}
