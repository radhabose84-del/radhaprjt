using Contracts.Common;
using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IRecurringGeneration;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IRecurringJournalTemplate;
using FinanceManagement.Domain.Common;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.JournalMaster.RecurringJournalTemplate.Commands.GenerateRecurringTemplateNow
{
    public class GenerateRecurringTemplateNowCommandHandler : IRequestHandler<GenerateRecurringTemplateNowCommand, ApiResponseDTO<int>>
    {
        private readonly IRecurringJournalGenerationService _generationService;
        private readonly IRecurringJournalTemplateQueryRepository _templateQueryRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMediator _mediator;

        public GenerateRecurringTemplateNowCommandHandler(
            IRecurringJournalGenerationService generationService,
            IRecurringJournalTemplateQueryRepository templateQueryRepository,
            IIPAddressService ipAddressService,
            IMediator mediator)
        {
            _generationService = generationService;
            _templateQueryRepository = templateQueryRepository;
            _ipAddressService = ipAddressService;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<int>> Handle(GenerateRecurringTemplateNowCommand request, CancellationToken cancellationToken)
        {
            var companyId = _ipAddressService.GetCompanyId()
                ?? throw new ExceptionRules("No active company in session.");

            // Only an active, APPROVED template can generate vouchers — a deleted/Pending/Rejected one is blocked.
            var info = await _templateQueryRepository.GetScheduleInfoAsync(request.TemplateId);
            if (info == null || info.IsDeleted)
                return new ApiResponseDTO<int> { IsSuccess = false, Message = "Recurring template not found.", Data = 0 };

            if (!string.Equals(info.StatusCode, MiscEnumEntity.Approved, StringComparison.OrdinalIgnoreCase))
                return new ApiResponseDTO<int>
                {
                    IsSuccess = false,
                    Message = $"Recurring template is not approved (status: {info.StatusCode}). Journal vouchers can be generated only after the template is approved.",
                    Data = 0
                };

            // Generate the JV — never auto-posted; low-risk → APPROVED, high-risk → DRAFT + approval. The
            // accounting period is resolved from the voucher date inside the service.
            var journalId = await _generationService.GenerateForTemplateAsync(
                companyId, request.TemplateId, request.VoucherDate, cancellationToken);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "GenerateNow",
                actionCode: "RECURRING_TEMPLATE_GENERATE",
                actionName: request.TemplateId.ToString(),
                details: journalId > 0
                    ? $"Recurring template {request.TemplateId} generated journal {journalId}."
                    : $"Recurring template {request.TemplateId} generate skipped (already generated or period not open).",
                module: "RecurringJournalTemplate"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = journalId > 0,
                Message = journalId > 0
                    ? "Recurring journal generated successfully."
                    : "Nothing generated — already generated for this period, or the period is not open.",
                Data = journalId
            };
        }
    }
}
