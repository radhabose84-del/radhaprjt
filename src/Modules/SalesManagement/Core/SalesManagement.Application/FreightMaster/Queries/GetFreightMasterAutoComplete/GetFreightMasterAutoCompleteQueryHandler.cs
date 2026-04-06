using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IFreightMaster;
using SalesManagement.Application.FreightMaster.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.FreightMaster.Queries.GetFreightMasterAutoComplete
{
    public class GetFreightMasterAutoCompleteQueryHandler : IRequestHandler<GetFreightMasterAutoCompleteQuery, IReadOnlyList<FreightMasterLookupDto>>
    {
        private readonly IFreightMasterQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetFreightMasterAutoCompleteQueryHandler(IFreightMasterQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<FreightMasterLookupDto>> Handle(GetFreightMasterAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.AutocompleteAsync(request.Term ?? string.Empty, cancellationToken);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetFreightMasterAutoCompleteQuery",
                actionName: result.Count.ToString(),
                details: "FreightMaster details was fetched.",
                module: "FreightMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
