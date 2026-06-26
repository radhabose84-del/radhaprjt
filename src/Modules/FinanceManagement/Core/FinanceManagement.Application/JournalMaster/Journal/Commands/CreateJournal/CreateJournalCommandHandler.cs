using System.Text.Json;
using AutoMapper;
using Contracts.Commands.Workflow;
using Contracts.Common;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Workflow;
using FinanceManagement.Application.Common.Interfaces.IOutbox;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IJournal;
using FinanceManagement.Application.JournalMaster.Dto;
using FinanceManagement.Domain.Common;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.JournalMaster.Journal.Commands.CreateJournal
{
    public class CreateJournalCommandHandler : IRequestHandler<CreateJournalCommand, ApiResponseDTO<int>>
    {
        private readonly IJournalCommandRepository _commandRepository;
        private readonly IJournalQueryRepository _queryRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly IFinancialYearLookup _financialYearLookup;
        private readonly IWorkflowLookup _workflowLookup;
        private readonly IOutboxEventPublisher _outboxEventPublisher;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateJournalCommandHandler(
            IJournalCommandRepository commandRepository,
            IJournalQueryRepository queryRepository,
            IIPAddressService ipAddressService,
            IFinancialYearLookup financialYearLookup,
            IWorkflowLookup workflowLookup,
            IOutboxEventPublisher outboxEventPublisher,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _ipAddressService = ipAddressService;
            _financialYearLookup = financialYearLookup;
            _workflowLookup = workflowLookup;
            _outboxEventPublisher = outboxEventPublisher;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateJournalCommand request, CancellationToken cancellationToken)
        {
            var companyId = _ipAddressService.GetCompanyId()
                ?? throw new ExceptionRules("No active company in session.");

            var period = await _queryRepository.GetOpenPeriodByDateAsync(companyId, request.VoucherDate)
                ?? throw new ExceptionRules("Voucher date is not within an open accounting period.");

            var statusId = await _queryRepository.GetStatusIdAsync("DRAFT");
            var sourceId = await _queryRepository.GetSourceIdAsync("MANUAL");

            var entity = _mapper.Map<FinanceManagement.Domain.Entities.JournalHeader>(request);
            entity.CompanyId = companyId;
            entity.UnitId = _ipAddressService.GetUnitId();
            entity.AccountingPeriodId = period.PeriodId;
            entity.FinancialYearId = period.FinancialYearId;
            entity.StatusId = statusId;
            entity.SourceId = sourceId;
            entity.IsReversal = false;

            entity.Details = BuildLines(request);
            entity.TotalDr = entity.Details.Sum(l => l.DrAmount);
            entity.TotalCr = entity.Details.Sum(l => l.CrAmount);

            // Voucher number is allocated at create time (US-GL01-01) — resolve the fiscal-year name for the format.
            var fy = await _financialYearLookup.GetByIdAsync(period.FinancialYearId, cancellationToken);
            if (string.IsNullOrEmpty(fy?.FinancialYearName))
                throw new ExceptionRules("Financial year could not be resolved for voucher numbering.");

            var newId = await _commandRepository.CreateAsync(entity, fy.FinancialYearName, _ipAddressService.GetUserId());

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "JOURNAL_CREATE",
                actionName: newId.ToString(),
                details: $"Journal voucher draft created successfully with Id {newId}.",
                module: "Journal"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            // A manual voucher routes through approval when a journal approval workflow is configured for the
            // unit; otherwise the draft is directly postable (US-GL01-06B / 07). The BackgroundService Workflow
            // module consumes the request and sends back UpdateApprovedRejectedFinanceCommand, which the Finance
            // ApprovedRejectedConsumer applies (Approved → APPROVED, Rejected → DRAFT).
            var unitId = entity.UnitId ?? 0;
            var workflowConfigured = await _workflowLookup.IsApproveWorkflowConfigureAsync(
                MiscEnumEntity.JournalVoucher, unitId, 0);

            var message = "Journal voucher saved as draft.";
            if (workflowConfigured)
            {
                await RaiseApprovalRequestAsync(newId, unitId, cancellationToken);
                message = "Journal voucher saved as draft and submitted for approval.";
            }

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = message,
                Data = newId
            };
        }

        private async Task RaiseApprovalRequestAsync(int journalId, int unitId, CancellationToken cancellationToken)
        {
            // Build the workflow payload from the persisted voucher. sp_EvaluateApproval reads $.Header.UnitId.
            var workFlowEntity = await _queryRepository.GetByIdAsync(journalId);
            if (workFlowEntity != null)
                workFlowEntity.UnitId = unitId;

            var reverseMap = new JournalApprovalReverseDto
            {
                Header = workFlowEntity,
                Lines = null
            };
            var serializedPayload = JsonSerializer.Serialize(reverseMap);

            var correlationId = Guid.NewGuid();
            var approvalRequest = new CreateApprovalRequestCommand
            {
                CorrelationId = correlationId,
                ModuleTypeName = MiscEnumEntity.JournalVoucher,
                ModuleTransactionId = journalId,
                Payload = serializedPayload
            };
            await _outboxEventPublisher.ScheduleAsync(approvalRequest, correlationId, cancellationToken);
        }

        private static List<FinanceManagement.Domain.Entities.JournalDetail> BuildLines(CreateJournalCommand request)
        {
            var lineNo = 0;
            return request.Lines.Select(l =>
            {
                var rate = l.ExchangeRate ?? 1m;
                return new FinanceManagement.Domain.Entities.JournalDetail
                {
                    LineNo = ++lineNo,
                    GlAccountId = l.GlAccountId,
                    DrAmount = l.DrAmount,
                    CrAmount = l.CrAmount,
                    CurrencyId = l.CurrencyId,
                    ExchangeRate = l.ExchangeRate,
                    BaseDrAmount = l.DrAmount * rate,
                    BaseCrAmount = l.CrAmount * rate,
                    CostCentreId = l.CostCentreId,
                    ProfitCentreId = l.ProfitCentreId,
                    LineNarration = l.LineNarration,
                    ReferenceDocNo = l.ReferenceDocNo
                };
            }).ToList();
        }
    }
}
