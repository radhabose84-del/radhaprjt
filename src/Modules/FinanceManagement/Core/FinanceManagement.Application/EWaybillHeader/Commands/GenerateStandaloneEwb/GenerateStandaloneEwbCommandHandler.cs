using System.Globalization;
using Contracts.Common;
using FinanceManagement.Application.Common.Interfaces.IEInvoiceHeader;
using FinanceManagement.Application.Common.Interfaces.IEWaybillHeader;
using FinanceManagement.Application.EInvoiceHeader.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.EWaybillHeader.Commands.GenerateStandaloneEwb
{
    public class GenerateStandaloneEwbCommandHandler
        : IRequestHandler<GenerateStandaloneEwbCommand, ApiResponseDTO<NicEwbResultDto>>
    {
        private readonly INicEInvoiceService _nicService;
        private readonly IEWaybillHeaderCommandRepository _commandRepo;
        private readonly IMediator _mediator;

        public GenerateStandaloneEwbCommandHandler(
            INicEInvoiceService nicService,
            IEWaybillHeaderCommandRepository commandRepo,
            IMediator mediator)
        {
            _nicService = nicService;
            _commandRepo = commandRepo;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<NicEwbResultDto>> Handle(
            GenerateStandaloneEwbCommand request, CancellationToken cancellationToken)
        {
            var nicResult = await _nicService.GenerateStandaloneEwbAsync(request.Payload, cancellationToken);

            if (nicResult.IsSuccess)
            {
                var ewbNumberStr = nicResult.EwbNo?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
                var generatedAt = TryParseNicDate(nicResult.EwbDate);
                var validUpto   = TryParseNicDate(nicResult.EwbValidTill);

                await _commandRepo.UpdateAfterNicSuccessAsync(
                    request.EWaybillHeaderId, ewbNumberStr, generatedAt, validUpto, cancellationToken);

                var auditEvent = new AuditLogsDomainEvent(
                    actionDetail: "Generate",
                    actionCode: "STANDALONE_EWB_GENERATED",
                    actionName: ewbNumberStr,
                    details: $"Standalone e-Waybill {ewbNumberStr} generated for EWaybillHeader Id {request.EWaybillHeaderId}.",
                    module: "EWaybillHeader");
                await _mediator.Publish(auditEvent, cancellationToken);

                return new ApiResponseDTO<NicEwbResultDto>
                {
                    IsSuccess = true,
                    Message = $"e-Waybill {ewbNumberStr} generated.",
                    Data = nicResult
                };
            }

            // Truncate to fit Finance.EWaybillHeader column constraints
            // (ErrorCode varchar(20), ErrorMessage varchar(500)). Any service-side
            // diagnostic prefixes can otherwise overshoot; truncating here means
            // the row update can never fail on column-length, regardless of caller.
            var safeErrorCode    = Truncate(nicResult.ErrorCode,    20);
            var safeErrorMessage = Truncate(nicResult.ErrorMessage, 500);

            await _commandRepo.UpdateAfterNicFailureAsync(
                request.EWaybillHeaderId, safeErrorCode, safeErrorMessage, cancellationToken);

            return new ApiResponseDTO<NicEwbResultDto>
            {
                IsSuccess = false,
                Message = nicResult.ErrorMessage ?? "NIC e-Waybill generation failed.",
                Data = nicResult
            };
        }

        private static string? Truncate(string? value, int maxLength) =>
            string.IsNullOrEmpty(value) || value.Length <= maxLength ? value : value[..maxLength];

        // NIC returns dates as "dd/MM/yyyy hh:mm:ss tt" (or sometimes ISO).
        // Parse defensively — return null on any format mismatch rather than throwing,
        // so a date-format change at NIC doesn't break the success path entirely.
        private static DateTimeOffset? TryParseNicDate(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;

            string[] formats =
            {
                "dd/MM/yyyy hh:mm:ss tt",
                "dd/MM/yyyy HH:mm:ss",
                "dd-MM-yyyy hh:mm:ss tt",
                "yyyy-MM-ddTHH:mm:ss",
                "yyyy-MM-dd HH:mm:ss"
            };

            if (DateTimeOffset.TryParseExact(raw, formats, CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeLocal, out var parsed))
                return parsed;

            return DateTimeOffset.TryParse(raw, CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeLocal, out var fallback) ? fallback : null;
        }
    }
}
