using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IMiscMaster;
using SalesManagement.Application.Common.Interfaces.IProformaInvoice;
using SalesManagement.Domain.Common;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.ProformaInvoice.Commands.UpdateProformaPayment
{
    public class UpdateProformaPaymentCommandHandler : IRequestHandler<UpdateProformaPaymentCommand, ApiResponseDTO<int>>
    {
        private readonly IProformaInvoiceCommandRepository _commandRepository;
        private readonly IProformaInvoiceQueryRepository _queryRepository;
        private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;
        private readonly IMediator _mediator;

        public UpdateProformaPaymentCommandHandler(
            IProformaInvoiceCommandRepository commandRepository,
            IProformaInvoiceQueryRepository queryRepository,
            IMiscMasterQueryRepository miscMasterQueryRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _miscMasterQueryRepository = miscMasterQueryRepository;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<int>> Handle(UpdateProformaPaymentCommand request, CancellationToken cancellationToken)
        {
            // Determine auto-status based on payment amount
            var proformaAmount = await _queryRepository.GetProformaAmountAsync(request.Id);
            int? statusId = null;

            if (request.PaymentReceivedAmount >= proformaAmount)
            {
                var paidStatus = await _miscMasterQueryRepository.GetMiscMasterByName(
                    MiscEnumEntity.ProformaInvoiceStatus, MiscEnumEntity.ProformaStatusPaid);
                statusId = paidStatus?.Id;
            }
            else if (request.PaymentReceivedAmount > 0)
            {
                var partialStatus = await _miscMasterQueryRepository.GetMiscMasterByName(
                    MiscEnumEntity.ProformaInvoiceStatus, MiscEnumEntity.ProformaStatusPartiallyPaid);
                statusId = partialStatus?.Id;
            }

            var result = await _commandRepository.UpdatePaymentAsync(request.Id, request.PaymentReceivedAmount, statusId);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "UpdatePayment",
                actionCode: "PROFORMA_PAYMENT_UPDATE",
                actionName: request.Id.ToString(),
                details: $"Proforma Invoice {request.Id} payment updated to {request.PaymentReceivedAmount}.",
                module: "ProformaInvoice");
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Payment updated successfully.",
                Data = result
            };
        }
    }
}
