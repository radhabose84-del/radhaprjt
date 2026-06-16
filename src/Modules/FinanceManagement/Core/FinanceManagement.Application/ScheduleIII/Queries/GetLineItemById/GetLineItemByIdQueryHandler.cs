using FinanceManagement.Application.Common.Interfaces.IScheduleIII;
using FinanceManagement.Application.ScheduleIII.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.ScheduleIII.Queries.GetLineItemById
{
    public class GetLineItemByIdQueryHandler : IRequestHandler<GetLineItemByIdQuery, ScheduleIIILineItemDto?>
    {
        private readonly IScheduleIIIQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetLineItemByIdQueryHandler(
            IScheduleIIIQueryRepository queryRepository,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<ScheduleIIILineItemDto?> Handle(GetLineItemByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetLineItemByIdAsync(request.Id);

            if (result == null)
                return null;

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetLineItemByIdQuery",
                actionName: result.Id.ToString(),
                details: $"Schedule III line item {result.Id} was fetched.",
                module: "ScheduleIIILineItem"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
