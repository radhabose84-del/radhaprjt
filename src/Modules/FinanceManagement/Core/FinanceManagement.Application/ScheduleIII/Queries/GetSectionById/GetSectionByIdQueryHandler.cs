using FinanceManagement.Application.Common.Interfaces.IScheduleIII;
using FinanceManagement.Application.ScheduleIII.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.ScheduleIII.Queries.GetSectionById
{
    public class GetSectionByIdQueryHandler : IRequestHandler<GetSectionByIdQuery, ScheduleIIISectionDto?>
    {
        private readonly IScheduleIIIQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetSectionByIdQueryHandler(IScheduleIIIQueryRepository queryRepository, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<ScheduleIIISectionDto?> Handle(GetSectionByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetSectionByIdAsync(request.Id);

            if (result == null)
                return null;

            await _mediator.Publish(new AuditLogsDomainEvent(
                actionDetail: "GetById", actionCode: "GetSectionByIdQuery",
                actionName: result.Id.ToString(),
                details: $"Schedule III section {result.Id} was fetched.",
                module: "ScheduleIIISection"), cancellationToken);

            return result;
        }
    }
}
