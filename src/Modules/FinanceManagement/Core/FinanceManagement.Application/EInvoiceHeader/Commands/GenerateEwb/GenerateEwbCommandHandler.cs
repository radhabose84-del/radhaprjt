using Contracts.Common;
using FinanceManagement.Application.Common.Interfaces.IEInvoiceHeader;
using FinanceManagement.Application.EInvoiceHeader.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.EInvoiceHeader.Commands.GenerateEwb
{
    public class GenerateEwbCommandHandler
        : IRequestHandler<GenerateEwbCommand, ApiResponseDTO<NicEwbResultDto>>
    {
        private readonly IEInvoiceHeaderCommandRepository _commandRepository;
        private readonly INicEInvoiceService _nicEInvoiceService;
        private readonly IMediator _mediator;

        public GenerateEwbCommandHandler(
            IEInvoiceHeaderCommandRepository commandRepository,
            INicEInvoiceService nicEInvoiceService,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _nicEInvoiceService = nicEInvoiceService;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<NicEwbResultDto>> Handle(
            GenerateEwbCommand request,
            CancellationToken cancellationToken)
        {
            var result = await _nicEInvoiceService.GenerateEwbAsync(
                request.EInvoiceHeaderId,
                request.TransporterId ?? string.Empty,
                request.TransporterName ?? string.Empty,
                request.TransMode ?? "1",
                request.Distance,
                request.TransDocNo ?? string.Empty,
                request.TransDocDt ?? string.Empty,
                request.VehicleNo ?? string.Empty,
                request.VehicleType ?? "R",
                cancellationToken);

            if (result.IsSuccess)
            {
                await _commandRepository.UpdateEwbDetailsAsync(
                    request.EInvoiceHeaderId,
                    result.EwbNo,
                    result.EwbDate,
                    result.EwbValidTill,
                    cancellationToken);
            }

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: result.IsSuccess ? "GenerateEwb" : "GenerateEwbFailed",
                actionCode: "EINVOICE_GENERATE_EWB",
                actionName: request.EInvoiceHeaderId.ToString(),
                details: result.IsSuccess
                    ? $"E-Waybill generated for EInvoiceHeader {request.EInvoiceHeaderId}: EwbNo {result.EwbNo}"
                    : $"E-Waybill generation failed for EInvoiceHeader {request.EInvoiceHeaderId}: {result.ErrorMessage}",
                module: "EInvoiceHeader");

            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<NicEwbResultDto>
            {
                IsSuccess = result.IsSuccess,
                Message = result.IsSuccess
                    ? "E-Waybill generated successfully."
                    : result.ErrorMessage ?? "E-Waybill generation failed.",
                Data = result
            };
        }
    }
}
