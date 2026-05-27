using AutoMapper;
using MediatR;
using QCManagement.Application.Common.Interfaces.IMiscMaster;
using QCManagement.Application.MiscMaster.Dto;
using QCManagement.Domain.Events;

namespace QCManagement.Application.MiscMaster.Queries.GetMiscMasterAutoComplete
{
    public class GetMiscMasterAutoCompleteQueryHandler : IRequestHandler<GetMiscMasterAutoCompleteQuery, IReadOnlyList<MiscMasterLookupDto>>
    {
        private readonly IMiscMasterQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetMiscMasterAutoCompleteQueryHandler(IMiscMasterQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<MiscMasterLookupDto>> Handle(GetMiscMasterAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.AutocompleteAsync(request.Term, request.MiscTypeCode, cancellationToken);
            var miscMasters = _mapper.Map<List<MiscMasterLookupDto>>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetMiscMasterAutoCompleteQuery",
                actionName: miscMasters.Count.ToString(),
                details: "MiscMaster details was fetched.",
                module: "MiscMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return miscMasters;
        }
    }
}
