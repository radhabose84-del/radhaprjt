using FinanceManagement.Application.Common.Interfaces.IScheduleIII;
using FinanceManagement.Application.ScheduleIII.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.ScheduleIII.Queries.GetScheduleIIIById
{
    public class GetScheduleIIIByIdQueryHandler : IRequestHandler<GetScheduleIIIByIdQuery, ScheduleIIIStructureDto?>
    {
        private readonly IScheduleIIIQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetScheduleIIIByIdQueryHandler(IScheduleIIIQueryRepository queryRepository, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<ScheduleIIIStructureDto?> Handle(GetScheduleIIIByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.Id);

            if (result == null)
                return null;

            await _mediator.Publish(new AuditLogsDomainEvent(
                actionDetail: "GetById", actionCode: "GetScheduleIIIByIdQuery",
                actionName: result.Id.ToString(),
                details: $"Schedule III structure {result.Id} (full tree) was fetched.",
                module: "ScheduleIIIStructure"), cancellationToken);

            return result;
        }
    }
}
