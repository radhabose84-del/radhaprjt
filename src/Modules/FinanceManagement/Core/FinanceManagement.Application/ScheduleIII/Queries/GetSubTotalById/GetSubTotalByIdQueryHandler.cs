using FinanceManagement.Application.Common.Interfaces.IScheduleIII;
using FinanceManagement.Application.ScheduleIII.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.ScheduleIII.Queries.GetSubTotalById
{
    public class GetSubTotalByIdQueryHandler : IRequestHandler<GetSubTotalByIdQuery, ScheduleIIISubTotalDto?>
    {
        private readonly IScheduleIIIQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetSubTotalByIdQueryHandler(IScheduleIIIQueryRepository queryRepository, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<ScheduleIIISubTotalDto?> Handle(GetSubTotalByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetSubTotalByIdAsync(request.Id);

            if (result == null)
                return null;

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetSubTotalByIdQuery",
                actionName: result.Id.ToString(),
                details: $"Schedule III sub-total {result.Id} was fetched.",
                module: "ScheduleIIISubTotal"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
