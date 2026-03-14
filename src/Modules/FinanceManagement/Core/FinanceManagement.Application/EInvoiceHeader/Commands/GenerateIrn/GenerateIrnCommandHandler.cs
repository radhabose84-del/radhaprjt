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
            var result = await _nicEInvoiceService.GenerateIrnAsync(
                request.EInvoiceHeaderId, cancellationToken);

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

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: result.IsSuccess ? "GenerateIrn" : "GenerateIrnFailed",
                actionCode: "EINVOICE_GENERATE_IRN",
                actionName: request.EInvoiceHeaderId.ToString(),
                details: result.IsSuccess
                    ? $"IRN generated for EInvoiceHeader {request.EInvoiceHeaderId}: {result.Irn}"
                    : $"IRN generation failed for EInvoiceHeader {request.EInvoiceHeaderId}: {result.ErrorMessage}",
                module: "EInvoiceHeader");

            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<NicIrnResultDto>
            {
                IsSuccess = result.IsSuccess,
                Message = result.IsSuccess
                    ? "IRN generated successfully."
                    : result.ErrorMessage ?? "IRN generation failed.",
                Data = result
            };
        }
    }
}
