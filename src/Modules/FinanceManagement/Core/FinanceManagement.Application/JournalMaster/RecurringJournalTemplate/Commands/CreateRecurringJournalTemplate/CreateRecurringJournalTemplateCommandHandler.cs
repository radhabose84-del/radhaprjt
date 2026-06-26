using System.Text.Json;
using AutoMapper;
using Contracts.Commands.Workflow;
using Contracts.Common;
using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IOutbox;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IRecurringJournalTemplate;
using FinanceManagement.Domain.Common;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.JournalMaster.RecurringJournalTemplate.Commands.CreateRecurringJournalTemplate
{
    public class CreateRecurringJournalTemplateCommandHandler : IRequestHandler<CreateRecurringJournalTemplateCommand, ApiResponseDTO<int>>
    {
        private readonly IRecurringJournalTemplateCommandRepository _commandRepository;
        private readonly IRecurringJournalTemplateQueryRepository _queryRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly IOutboxEventPublisher _outboxEventPublisher;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateRecurringJournalTemplateCommandHandler(
            IRecurringJournalTemplateCommandRepository commandRepository,
            IRecurringJournalTemplateQueryRepository queryRepository,
            IIPAddressService ipAddressService,
            IOutboxEventPublisher outboxEventPublisher,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _ipAddressService = ipAddressService;
            _outboxEventPublisher = outboxEventPublisher;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateRecurringJournalTemplateCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<FinanceManagement.Domain.Entities.RecurringJournalTemplateHeader>(request);
            entity.CompanyId = _ipAddressService.GetCompanyId() ?? 0;   // unattended auto-post job has no session
            entity.UnitId = _ipAddressService.GetUnitId() ?? 0;        // stamped onto journals the job generates
            entity.Lines = BuildLines(request);

            // EVERY template goes through approval (US-GL01-11) — created Pending; the auto-post Hangfire job is
            // scheduled only once the template is approved (ApprovedRejectedConsumer → SyncAsync). LowRisk/AutoPost
            // affect only the GENERATED journals (at generate-now / Hangfire time), not the template's own approval.
            entity.StatusId = await _queryRepository.GetApprovalStatusIdAsync(MiscEnumEntity.Pending);

            var newId = await _commandRepository.CreateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "RECURRING_TEMPLATE_CREATE",
                actionName: request.TemplateName ?? string.Empty,
                details: $"Recurring journal template '{request.TemplateName}' created with Id {newId}.",
                module: "RecurringJournalTemplate"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            await RaiseApprovalRequestAsync(newId, request.TemplateName, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Recurring journal template created and submitted for approval.",
                Data = newId
            };
        }

        private async Task RaiseApprovalRequestAsync(int templateId, string? templateName, CancellationToken cancellationToken)
        {
            // The template carries no UnitId; sp_EvaluateApproval reads $.Header.UnitId → supply it from the token.
            var unitId = _ipAddressService.GetUnitId()
                ?? throw new ExceptionRules("No active unit in session — required for the approval workflow.");

            var reverseMap = new RecurringTemplateApprovalReverseDto
            {
                Header = new RecurringTemplateApprovalHeaderDto { Id = templateId, UnitId = unitId, TemplateName = templateName },
                Lines = null
            };
            var serializedPayload = JsonSerializer.Serialize(reverseMap);

            var correlationId = Guid.NewGuid();
            await _outboxEventPublisher.ScheduleAsync(new CreateApprovalRequestCommand
            {
                CorrelationId = correlationId,
                ModuleTypeName = MiscEnumEntity.RecurringJournalTemplate,
                ModuleTransactionId = templateId,
                Payload = serializedPayload
            }, correlationId, cancellationToken);
        }

        private static List<FinanceManagement.Domain.Entities.RecurringJournalTemplateDetail> BuildLines(CreateRecurringJournalTemplateCommand request)
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
