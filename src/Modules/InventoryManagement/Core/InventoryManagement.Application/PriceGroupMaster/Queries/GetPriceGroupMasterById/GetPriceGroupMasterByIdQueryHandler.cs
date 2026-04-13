using InventoryManagement.Application.Common.Interfaces.IPriceGroupMaster;
using InventoryManagement.Application.PriceGroupMaster.Dto;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.Application.PriceGroupMaster.Queries.GetPriceGroupMasterById
{
    public class GetPriceGroupMasterByIdQueryHandler : IRequestHandler<GetPriceGroupMasterByIdQuery, PriceGroupMasterDto?>
    {
        private readonly IPriceGroupMasterQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetPriceGroupMasterByIdQueryHandler(
            IPriceGroupMasterQueryRepository queryRepository,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<PriceGroupMasterDto?> Handle(GetPriceGroupMasterByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.Id);

            if (result == null)
                return null;

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "PRICEGROUP_GETBYID",
                actionName: result.Id.ToString(),
                details: $"Price Group details {result.Id} were fetched.",
                module: "PriceGroupMaster"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return result;
        }
    }
}
