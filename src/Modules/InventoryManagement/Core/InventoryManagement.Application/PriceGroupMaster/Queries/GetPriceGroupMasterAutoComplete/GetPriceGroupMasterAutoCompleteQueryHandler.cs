using Contracts.Dtos.Lookups.Inventory;
using InventoryManagement.Application.Common.Interfaces.IPriceGroupMaster;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.Application.PriceGroupMaster.Queries.GetPriceGroupMasterAutoComplete
{
    public class GetPriceGroupMasterAutoCompleteQueryHandler : IRequestHandler<GetPriceGroupMasterAutoCompleteQuery, IReadOnlyList<PriceGroupMasterLookupDto>>
    {
        private readonly IPriceGroupMasterQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetPriceGroupMasterAutoCompleteQueryHandler(
            IPriceGroupMasterQueryRepository queryRepository,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<PriceGroupMasterLookupDto>> Handle(GetPriceGroupMasterAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.AutocompleteAsync(request.Term ?? string.Empty, cancellationToken);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "PRICEGROUP_AUTOCOMPLETE",
                actionName: result.Count.ToString(),
                details: "Price Group autocomplete records were fetched.",
                module: "PriceGroupMaster"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return result;
        }
    }
}
