using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IMixCodeMaster;
using PurchaseManagement.Application.MixCodeMaster.Dto;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.MixCodeMaster.Queries.GetMixCodeMasterAutoComplete
{
    public class GetMixCodeMasterAutoCompleteQueryHandler : IRequestHandler<GetMixCodeMasterAutoCompleteQuery, IReadOnlyList<MixCodeMasterLookupDto>>
    {
        private readonly IMixCodeMasterQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetMixCodeMasterAutoCompleteQueryHandler(IMixCodeMasterQueryRepository queryRepository, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<MixCodeMasterLookupDto>> Handle(GetMixCodeMasterAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.AutocompleteAsync(request.Term ?? string.Empty, cancellationToken);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetMixCodeMasterAutoCompleteQuery",
                actionName: result.Count.ToString(),
                details: "MixCodeMaster details was fetched.",
                module: "MixCodeMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
