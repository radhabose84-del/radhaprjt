using Contracts.Common;
using FinanceManagement.Application.Common.Interfaces.IEInvoiceHeader;
using FinanceManagement.Application.EInvoiceHeader.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.EInvoiceHeader.Commands.CancelEwb
{
    public class CancelEwbCommandHandler
        : IRequestHandler<CancelEwbCommand, ApiResponseDTO<NicCancelEwbResultDto>>
    {
        private readonly IEInvoiceHeaderCommandRepository _commandRepository;
        private readonly INicEInvoiceService _nicEInvoiceService;
        private readonly IMediator _mediator;

        public CancelEwbCommandHandler(
            IEInvoiceHeaderCommandRepository commandRepository,
            INicEInvoiceService nicEInvoiceService,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _nicEInvoiceService = nicEInvoiceService;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<NicCancelEwbResultDto>> Handle(
            CancelEwbCommand request,
            CancellationToken cancellationToken)
        {
            var result = await _nicEInvoiceService.CancelEwbAsync(
                request.EInvoiceHeaderId,
                request.CancelRsnCode,
                request.CancelRmrk,
                cancellationToken);

            if (result.IsSuccess)
            {
                // Clear EWB details on success
                await _commandRepository.UpdateEwbDetailsAsync(
                    request.EInvoiceHeaderId,
                    null, null, null,
                    cancellationToken);
            }

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: result.IsSuccess ? "CancelEwb" : "CancelEwbFailed",
                actionCode: "EINVOICE_CANCEL_EWB",
                actionName: request.EInvoiceHeaderId.ToString(),
                details: result.IsSuccess
                    ? $"e-Waybill cancelled for EInvoiceHeader {request.EInvoiceHeaderId}. CancelDate: {result.CancelDate}"
                    : $"e-Waybill cancellation failed: {result.ErrorMessage}",
                module: "EInvoiceHeader");

            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<NicCancelEwbResultDto>
            {
                IsSuccess = result.IsSuccess,
                Message = result.IsSuccess
                    ? "e-Waybill cancelled successfully."
                    : result.ErrorMessage ?? "e-Waybill cancellation failed.",
                Data = result
            };
        }
    }
}
