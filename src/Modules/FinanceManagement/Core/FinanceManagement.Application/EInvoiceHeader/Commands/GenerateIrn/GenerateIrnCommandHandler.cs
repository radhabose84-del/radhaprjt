using Contracts.Common;
using FinanceManagement.Application.Common.Interfaces.IEInvoiceHeader;
using FinanceManagement.Application.EInvoiceHeader.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.EInvoiceHeader.Commands.GenerateIrn
{
    public class GenerateIrnCommandHandler
        : IRequestHandler<GenerateIrnCommand, ApiResponseDTO<NicIrnResultDto>>
    {
        private readonly IEInvoiceHeaderCommandRepository _commandRepository;
        private readonly INicEInvoiceService _nicEInvoiceService;
        private readonly IMediator _mediator;

        public GenerateIrnCommandHandler(
            IEInvoiceHeaderCommandRepository commandRepository,
            INicEInvoiceService nicEInvoiceService,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _nicEInvoiceService = nicEInvoiceService;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<NicIrnResultDto>> Handle(
            GenerateIrnCommand request,
            CancellationToken cancellationToken)
        {
            // Build optional EWB transport details (Case 1: IRN + EWB together)
            EwbTransportDetails? ewbDetails = null;
            if (request.Distance.HasValue && request.Distance.Value > 0)
            {
                ewbDetails = new EwbTransportDetails
                {
                    TransId = request.TransId,
                    TransName = request.TransName,
                    TransMode = request.TransMode,
                    Distance = request.Distance.Value,
                    TransDocNo = request.TransDocNo,
                    TransDocDt = request.TransDocDt,
                    VehNo = request.VehNo,
                    VehType = request.VehType
                };
            }

            var result = await _nicEInvoiceService.GenerateIrnAsync(
                request.EInvoiceHeaderId, ewbDetails, cancellationToken);

            if (result.IsSuccess)
            {
                await _commandRepository.UpdateIrnDetailsAsync(
                    request.EInvoiceHeaderId,
                    result.Irn,
                    result.AckNo,
                    result.AckDate,
                    result.SignedInvoice,
                    result.SignedQRCode,
                    "Generated",
                    null,
                    null,
                    cancellationToken);

                // If e-Waybill was also generated (Case 1), update EWB details
                if (result.EwbNo.HasValue)
                {
                    await _commandRepository.UpdateEwbDetailsAsync(
                        request.EInvoiceHeaderId,
                        result.EwbNo,
                        result.EwbDate,
                        result.EwbValidTill,
                        cancellationToken);
                }
            }
            else
            {
                await _commandRepository.UpdateIrnDetailsAsync(
                    request.EInvoiceHeaderId,
                    null, null, null, null, null,
                    "Failed",
                    result.ErrorCode,
                    result.ErrorMessage,
                    cancellationToken);
            }

            var auditDetail = result.IsSuccess
                ? (result.EwbNo.HasValue ? "GenerateIrnWithEwb" : "GenerateIrn")
                : "GenerateIrnFailed";

            var auditMessage = result.IsSuccess
                ? $"IRN generated for EInvoiceHeader {request.EInvoiceHeaderId}: {result.Irn}"
                  + (result.EwbNo.HasValue ? $", EwbNo: {result.EwbNo}" : string.Empty)
                : $"IRN generation failed for EInvoiceHeader {request.EInvoiceHeaderId}: {result.ErrorMessage}";

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: auditDetail,
                actionCode: "EINVOICE_GENERATE_IRN",
                actionName: request.EInvoiceHeaderId.ToString(),
                details: auditMessage,
                module: "EInvoiceHeader");

            await _mediator.Publish(auditEvent, cancellationToken);

            var successMessage = result.IsSuccess
                ? (result.EwbNo.HasValue
                    ? "IRN and e-Waybill generated successfully."
                    : "IRN generated successfully.")
                : result.ErrorMessage ?? "IRN generation failed.";

            return new ApiResponseDTO<NicIrnResultDto>
            {
                IsSuccess = result.IsSuccess,
                Message = successMessage,
                Data = result
            };
        }
    }
}
