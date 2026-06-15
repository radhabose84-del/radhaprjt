using FinanceManagement.Application.Common.Interfaces.IScheduleIII;
using FinanceManagement.Application.ScheduleIII.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.ScheduleIII.Queries.Get03BDropdownPreview
{
    public class Get03BDropdownPreviewQueryHandler : IRequestHandler<Get03BDropdownPreviewQuery, Preview03BDto>
    {
        private readonly IScheduleIIIQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public Get03BDropdownPreviewQueryHandler(
            IScheduleIIIQueryRepository queryRepository,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<Preview03BDto> Handle(Get03BDropdownPreviewQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.Get03BPreviewAsync(request.StructureId);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Get03BDropdownPreview",
                actionCode: "Get03BDropdownPreviewQuery",
                actionName: (result.BalanceSheetLeaves.Count + result.ProfitAndLossLeaves.Count).ToString(),
                details: "Schedule III 03B dropdown preview was fetched.",
                module: "ScheduleIIIStructure"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
