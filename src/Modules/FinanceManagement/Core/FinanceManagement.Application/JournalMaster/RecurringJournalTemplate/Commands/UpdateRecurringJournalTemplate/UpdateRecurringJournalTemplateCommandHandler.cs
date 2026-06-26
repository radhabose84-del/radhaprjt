using AutoMapper;
using Contracts.Common;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IRecurringJournalTemplate;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.JournalMaster.RecurringJournalTemplate.Commands.UpdateRecurringJournalTemplate
{
    public class UpdateRecurringJournalTemplateCommandHandler : IRequestHandler<UpdateRecurringJournalTemplateCommand, ApiResponseDTO<int>>
    {
        private readonly IRecurringJournalTemplateCommandRepository _commandRepository;
        private readonly IRecurringTemplateScheduler _scheduler;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateRecurringJournalTemplateCommandHandler(
            IRecurringJournalTemplateCommandRepository commandRepository,
            IRecurringTemplateScheduler scheduler,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _scheduler = scheduler;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(UpdateRecurringJournalTemplateCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<FinanceManagement.Domain.Entities.RecurringJournalTemplateHeader>(request);
            entity.Lines = BuildLines(request);

            var updatedId = await _commandRepository.UpdateAsync(entity);

            // No approval flow on edit — the template's existing approval status is preserved (UpdateAsync does not
            // touch StatusId). Re-sync the Hangfire auto-post job: edits to frequency / start date / AutoPost
            // re-schedule it (only for an Approved + AutoPost template), and turning AutoPost off removes it.
            await _scheduler.SyncAsync(request.Id, cancellationToken);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "RECURRING_TEMPLATE_UPDATE",
                actionName: request.Id.ToString(),
                details: $"Recurring journal template with Id {request.Id} updated successfully.",
                module: "RecurringJournalTemplate"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Recurring journal template updated successfully.",
                Data = updatedId
            };
        }

        private static List<FinanceManagement.Domain.Entities.RecurringJournalTemplateDetail> BuildLines(UpdateRecurringJournalTemplateCommand request)
        {
            var lineNo = 0;
            return request.Lines.Select(l => new FinanceManagement.Domain.Entities.RecurringJournalTemplateDetail
            {
                LineNo = ++lineNo,
                GlAccountId = l.GlAccountId,
                DrAmount = l.DrAmount,
                CrAmount = l.CrAmount,
                AmountFormula = l.AmountFormula,
                CurrencyId = l.CurrencyId,
                ExchangeRate = l.ExchangeRate,
                CostCentreId = l.CostCentreId,
                ProfitCentreId = l.ProfitCentreId,
                LineNarration = l.LineNarration
            }).ToList();
        }
    }
}
