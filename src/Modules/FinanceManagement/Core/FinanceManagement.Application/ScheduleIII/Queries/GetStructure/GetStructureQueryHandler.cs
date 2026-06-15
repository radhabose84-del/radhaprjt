using FinanceManagement.Application.Common.Interfaces.IScheduleIII;
using FinanceManagement.Application.ScheduleIII.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.ScheduleIII.Queries.GetStructure
{
    public class GetStructureQueryHandler : IRequestHandler<GetStructureQuery, ScheduleIIIStructureDto?>
    {
        private readonly IScheduleIIIQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetStructureQueryHandler(
            IScheduleIIIQueryRepository queryRepository,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<ScheduleIIIStructureDto?> Handle(GetStructureQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetStructureAsync(request.CompanyId, request.DivisionId);

            if (result == null)
                return null;

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetStructure",
                actionCode: "GetStructureQuery",
                actionName: result.Id.ToString(),
                details: $"Schedule III structure {result.Id} was fetched.",
                module: "ScheduleIIIStructure"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
