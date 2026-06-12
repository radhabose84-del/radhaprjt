using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IRawMaterialPO;
using PurchaseManagement.Application.RawMaterialPO.Dto;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.RawMaterialPO.Queries.GetRawMaterialPOAutoComplete
{
    public class GetRawMaterialPOAutoCompleteQueryHandler : IRequestHandler<GetRawMaterialPOAutoCompleteQuery, IReadOnlyList<RawMaterialPOLookupDto>>
    {
        private readonly IRawMaterialPOQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetRawMaterialPOAutoCompleteQueryHandler(IRawMaterialPOQueryRepository queryRepository, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<RawMaterialPOLookupDto>> Handle(GetRawMaterialPOAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.AutocompleteAsync(request.Term ?? string.Empty, request.ShowAll, cancellationToken);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetRawMaterialPOAutoCompleteQuery",
                actionName: result.Count.ToString(),
                details: "Raw Material PO details was fetched.",
                module: "RawMaterialPO");
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
