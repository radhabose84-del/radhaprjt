using AutoMapper;
using FinanceManagement.Application.Common.Interfaces.IMiscTypeMaster;
using FinanceManagement.Application.MiscTypeMaster.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMasterAutoComplete
{
    public class GetMiscTypeMasterAutoCompleteQueryHandler : IRequestHandler<GetMiscTypeMasterAutoCompleteQuery, IReadOnlyList<MiscTypeMasterLookupDto>>
    {
        private readonly IMiscTypeMasterQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetMiscTypeMasterAutoCompleteQueryHandler(IMiscTypeMasterQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<MiscTypeMasterLookupDto>> Handle(GetMiscTypeMasterAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.AutocompleteAsync(request.Term, cancellationToken);
            var miscTypeMasters = _mapper.Map<List<MiscTypeMasterLookupDto>>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetMiscTypeMasterAutoCompleteQuery",
                actionName: miscTypeMasters.Count.ToString(),
                details: "MiscTypeMaster details was fetched.",
                module: "MiscTypeMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return miscTypeMasters;
        }
    }
}
