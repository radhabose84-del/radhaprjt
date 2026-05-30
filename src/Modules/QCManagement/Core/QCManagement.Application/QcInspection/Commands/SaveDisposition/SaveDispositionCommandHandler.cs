using Contracts.Common;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Purchase;
using MediatR;
using QCManagement.Application.Common.Interfaces.IQcInspection;
using QCManagement.Domain.Events;

namespace QCManagement.Application.QcInspection.Commands.SaveDisposition
{
    public class SaveDispositionCommandHandler : IRequestHandler<SaveDispositionCommand, ApiResponseDTO<int>>
    {
        private readonly IQcInspectionCommandRepository _commandRepository;
        private readonly IQcInspectionQueryRepository _queryRepository;
        private readonly IGrnLookup _grnLookup;
        private readonly IMediator _mediator;
        private readonly IIPAddressService _ipAddressService;

        public SaveDispositionCommandHandler(
            IQcInspectionCommandRepository commandRepository,
            IQcInspectionQueryRepository queryRepository,
            IGrnLookup grnLookup,
            IMediator mediator,
            IIPAddressService ipAddressService)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _grnLookup = grnLookup;
            _mediator = mediator;
            _ipAddressService = ipAddressService;
        }

        public async Task<ApiResponseDTO<int>> Handle(SaveDispositionCommand request, CancellationToken cancellationToken)
        {
            var statusCode = (request.QcStatusCode ?? string.Empty).Trim().ToUpperInvariant();

            var qcStatusId = await _queryRepository.GetQcStatusIdByCodeAsync(statusCode)
                ?? throw new ExceptionRules("Invalid QC Status.");

            var ctx = await _queryRepository.GetDispositionContextAsync(request.QcInspectionHdrId)
                ?? throw new ExceptionRules("Inspection not found.");

            var dispositionByUserId = _ipAddressService.GetUserId();
            var dispositionByName = _ipAddressService.GetUserName();

            await _commandRepository.SaveDispositionAsync(
                request.QcInspectionHdrId, qcStatusId,
                request.AcceptedQuantity, request.RejectedQuantity, request.DispositionRemarks,
                dispositionByUserId, dispositionByName,
                ctx.GrnHeaderId, ctx.GrnDetailId);

            var grn = await _grnLookup.GetByGrnDetailIdAsync(ctx.GrnDetailId, cancellationToken);

            var movementEvent = new QcDispositionCompletedDomainEvent(
                QcInspectionHdrId: request.QcInspectionHdrId,
                QcInspectionNo: ctx.QcInspectionNo ?? string.Empty,
                GrnHeaderId: ctx.GrnHeaderId,
                GrnDetailId: ctx.GrnDetailId,
                ItemId: grn?.ItemId ?? 0,
                QcStatusCode: statusCode,
                AcceptedQuantity: request.AcceptedQuantity,
                RejectedQuantity: request.RejectedQuantity,
                ReceivedUomId: ctx.ReceivedUomId,
                DispositionRemarks: request.DispositionRemarks);
            await _mediator.Publish(movementEvent, cancellationToken);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Disposition",
                actionCode: "QC_INSPECTION_DISPOSITION",
                actionName: request.QcInspectionHdrId.ToString(),
                details: $"QC Inspection {request.QcInspectionHdrId} disposed as '{statusCode}' (Accepted {request.AcceptedQuantity}, Rejected {request.RejectedQuantity}).",
                module: "QcInspection"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "QC disposition saved successfully.",
                Data = request.QcInspectionHdrId
            };
        }
    }
}
