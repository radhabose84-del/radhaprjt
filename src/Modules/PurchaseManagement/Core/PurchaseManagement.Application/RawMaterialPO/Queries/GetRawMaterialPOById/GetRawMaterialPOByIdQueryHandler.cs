using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IRawMaterialPO;
using PurchaseManagement.Application.RawMaterialPO.Dto;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.RawMaterialPO.Queries.GetRawMaterialPOById
{
    public class GetRawMaterialPOByIdQueryHandler : IRequestHandler<GetRawMaterialPOByIdQuery, RawMaterialPODto?>
    {
        private readonly IRawMaterialPOQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetRawMaterialPOByIdQueryHandler(IRawMaterialPOQueryRepository queryRepository, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<RawMaterialPODto?> Handle(GetRawMaterialPOByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.Id);

            if (result == null)
                return null;

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetRawMaterialPOByIdQuery",
                actionName: result.Id.ToString(),
                details: $"Raw Material PO details {result.Id} was fetched.",
                module: "RawMaterialPO");
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
