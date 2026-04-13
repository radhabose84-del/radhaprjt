using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesOrder;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesOrder.Queries.GetDiscountsBySalesGroup
{
    public class GetDiscountsBySalesGroupQueryHandler : IRequestHandler<GetDiscountsBySalesGroupQuery, List<DiscountsBySalesGroupDto>>
    {
        private readonly ISalesOrderQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetDiscountsBySalesGroupQueryHandler(
            ISalesOrderQueryRepository queryRepository,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<List<DiscountsBySalesGroupDto>> Handle(GetDiscountsBySalesGroupQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetDiscountsBySalesGroupAsync(request.SalesGroupId, request.SlabTypeId, request.PaymentTermId, cancellationToken);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetDiscountsBySalesGroupQuery",
                actionCode: "Get",
                actionName: $"SalesGroupId:{request.SalesGroupId},SlabTypeId:{request.SlabTypeId},PaymentTermId:{request.PaymentTermId}",
                details: $"Discounts for SalesGroup {request.SalesGroupId} + SlabType {request.SlabTypeId} + PaymentTerm {request.PaymentTermId} were fetched.",
                module: "SalesOrder");
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
