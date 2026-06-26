using System.Text.Json;
using Contracts.Commands.Workflow;
using Contracts.Common;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Workflow;
using FinanceManagement.Application.Common.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IOutbox;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IJournal;
using FinanceManagement.Application.JournalMaster.Dto;
using FinanceManagement.Domain.Common;
using FinanceManagement.Domain.Entities;
using FinanceManagement.Domain.Events;
using MediatR;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.Application.JournalMaster.Journal.Commands.CopyJournal
{
    public class CopyJournalCommandHandler : IRequestHandler<CopyJournalCommand, ApiResponseDTO<int>>
    {
        private readonly IJournalCommandRepository _commandRepository;
        private readonly IJournalQueryRepository _queryRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly ITimeZoneService _timeZoneService;
        private readonly IFinancialYearLookup _financialYearLookup;
        private readonly IWorkflowLookup _workflowLookup;
        private readonly IOutboxEventPublisher _outboxEventPublisher;
        private readonly IMediator _mediator;

        public CopyJournalCommandHandler(
            IJournalCommandRepository commandRepository,
            IJournalQueryRepository queryRepository,
            IIPAddressService ipAddressService,
            ITimeZoneService timeZoneService,
            IFinancialYearLookup financialYearLookup,
            IWorkflowLookup workflowLookup,
            IOutboxEventPublisher outboxEventPublisher,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _ipAddressService = ipAddressService;
            _timeZoneService = timeZoneService;
            _financialYearLookup = financialYearLookup;
            _workflowLookup = workflowLookup;
            _outboxEventPublisher = outboxEventPublisher;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<int>> Handle(CopyJournalCommand request, CancellationToken cancellationToken)
        {
            var source = await _queryRepository.GetByIdAsync(request.Id)
                ?? throw new ExceptionRules("Journal voucher not found.");

            var companyId = _ipAddressService.GetCompanyId()
                ?? throw new ExceptionRules("No active company in session.");

            // New draft dated today; resolve the open period for that date (same rule as a manual draft).
            var today = DateOnly.FromDateTime(_timeZoneService.GetCurrentTime().DateTime);
            var period = await _queryRepository.GetOpenPeriodByDateAsync(companyId, today)
                ?? throw new ExceptionRules("Today is not within an open accounting period; cannot create the copy.");

            var draftStatusId = await _queryRepository.GetStatusIdAsync("DRAFT");
            var sourceId = await _queryRepository.GetSourceIdAsync("MANUAL");

            var lineNo = 0;
            var copy = new JournalHeader
            {
                CompanyId = companyId,
                UnitId = _ipAddressService.GetUnitId(),
                VoucherTypeId = source.VoucherTypeId,
                VoucherDate = today,
                FinancialYearId = period.FinancialYearId,
                AccountingPeriodId = period.PeriodId,
                Narration = source.Narration,
                StatusId = draftStatusId,
                SourceId = sourceId,
                IsReversal = false,
                CopiedFromRef = source.VoucherNo,   // informational only — no posting/FK link
                TotalDr = source.TotalDr,
                TotalCr = source.TotalCr,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted,
                Details = source.Lines.Select(l => new JournalDetail
                {
                    LineNo = ++lineNo,
                    GlAccountId = l.GlAccountId,
                    DrAmount = l.DrAmount,
                    CrAmount = l.CrAmount,
                    CurrencyId = l.CurrencyId,
                    ExchangeRate = l.ExchangeRate,
                    BaseDrAmount = l.BaseDrAmount,
                    BaseCrAmount = l.BaseCrAmount,
                    CostCentreId = l.CostCentreId,
                    ProfitCentreId = l.ProfitCentreId,
                    LineNarration = l.LineNarration,
                    ReferenceDocNo = l.ReferenceDocNo,
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                }).ToList()
            };

            // Voucher number allocated at create time (same as a manual draft, US-GL01-01).
            var fy = await _financialYearLookup.GetByIdAsync(period.FinancialYearId, cancellationToken);
            if (string.IsNullOrEmpty(fy?.FinancialYearName))
                throw new ExceptionRules("Financial year could not be resolved for voucher numbering.");

            var newId = await _commandRepository.CreateAsync(copy, fy.FinancialYearName, _ipAddressService.GetUserId());

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Copy",
                actionCode: "JOURNAL_COPY",
                actionName: newId.ToString(),
                details: $"Journal voucher {request.Id} ({source.VoucherNo}) copied to a new draft with Id {newId}.",
                module: "Journal"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            // The copy is a manual draft — route it through approval when a journal approval workflow is
            // configured for the unit (same as CreateJournalCommandHandler); otherwise it's directly postable.
            var unitId = copy.UnitId ?? 0;
            var workflowConfigured = await _workflowLookup.IsApproveWorkflowConfigureAsync(
                MiscEnumEntity.JournalVoucher, unitId, 0);

            var message = "Journal copied to a new editable draft.";
            if (workflowConfigured)
            {
                await RaiseApprovalRequestAsync(newId, unitId, cancellationToken);
                message = "Journal copied to a new draft and submitted for approval.";
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
            var workFlowEntity = await _queryRepository.GetByIdAsync(journalId);
            if (workFlowEntity != null)
                workFlowEntity.UnitId = unitId;   // sp_EvaluateApproval reads $.Header.UnitId

            var reverseMap = new JournalApprovalReverseDto { Header = workFlowEntity, Lines = null };
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
    }
}
