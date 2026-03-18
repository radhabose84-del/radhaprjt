using Contracts.Common;
using FinanceManagement.Application.Common.Interfaces.IEInvoiceHeader;
using FinanceManagement.Application.EInvoiceHeader.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.EInvoiceHeader.Commands.CancelIrn
{
    public class CancelIrnCommandHandler
        : IRequestHandler<CancelIrnCommand, ApiResponseDTO<NicCancelIrnResultDto>>
    {
        private readonly IEInvoiceHeaderCommandRepository _commandRepository;
        private readonly INicEInvoiceService _nicEInvoiceService;
        private readonly IMediator _mediator;

        public CancelIrnCommandHandler(
            IEInvoiceHeaderCommandRepository commandRepository,
            INicEInvoiceService nicEInvoiceService,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _nicEInvoiceService = nicEInvoiceService;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<NicCancelIrnResultDto>> Handle(
            CancelIrnCommand request,
            CancellationToken cancellationToken)
        {
            var result = await _nicEInvoiceService.CancelIrnAsync(
                request.EInvoiceHeaderId,
                request.CnlRsn ?? "1",
                request.CnlRem,
                cancellationToken);

            if (result.IsSuccess)
            {
                await _commandRepository.UpdateIrnDetailsAsync(
                    request.EInvoiceHeaderId,
                    null, null, null, null, null,
                    "Cancelled",
                    null,
                    $"Cancelled on {result.CancelDate}. Reason: {request.CnlRsn}",
                    cancellationToken);
            }
            else
            {
                // On failure, only update status/error fields — preserve existing IRN data
                var errorMsg = result.ErrorMessage?.Length > 490
                    ? result.ErrorMessage[..490] + "..."
                    : result.ErrorMessage;

                await _commandRepository.UpdateIrnStatusAsync(
                    request.EInvoiceHeaderId,
                    "CancelFailed",
                    result.ErrorCode,
                    errorMsg,
                    cancellationToken);
            }

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: result.IsSuccess ? "CancelIrn" : "CancelIrnFailed",
                actionCode: "EINVOICE_CANCEL_IRN",
                actionName: request.EInvoiceHeaderId.ToString(),
                details: result.IsSuccess
                    ? $"IRN cancelled for EInvoiceHeader {request.EInvoiceHeaderId}. CancelDate: {result.CancelDate}"
                    : $"IRN cancellation failed: {result.ErrorMessage}",
                module: "EInvoiceHeader");

            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<NicCancelIrnResultDto>
            {
                IsSuccess = result.IsSuccess,
                Message = result.IsSuccess
                    ? "IRN cancelled successfully."
                    : result.ErrorMessage ?? "IRN cancellation failed.",
                Data = result
            };
        }
    }
}
