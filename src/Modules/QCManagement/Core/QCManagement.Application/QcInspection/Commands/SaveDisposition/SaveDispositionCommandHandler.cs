using Contracts.Common;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Purchase;
using MediatR;
using QCManagement.Application.Common.Interfaces.IQcInspection;
using QCManagement.Application.Common.Services;
using QCManagement.Domain.Events;

namespace QCManagement.Application.QcInspection.Commands.SaveDisposition
{
    public class SaveDispositionCommandHandler : IRequestHandler<SaveDispositionCommand, ApiResponseDTO<int>>
    {
        private readonly IQcInspectionCommandRepository _commandRepository;
        private readonly IQcInspectionQueryRepository _queryRepository;
        private readonly IInspectionEvaluator _evaluator;
        private readonly IGrnLookup _grnLookup;
        private readonly IMediator _mediator;
        private readonly IIPAddressService _ipAddressService;

        public SaveDispositionCommandHandler(
            IQcInspectionCommandRepository commandRepository,
            IQcInspectionQueryRepository queryRepository,
            IInspectionEvaluator evaluator,
            IGrnLookup grnLookup,
            IMediator mediator,
            IIPAddressService ipAddressService)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _evaluator = evaluator;
            _grnLookup = grnLookup;
            _mediator = mediator;
            _ipAddressService = ipAddressService;
        }

        public async Task<ApiResponseDTO<int>> Handle(SaveDispositionCommand request, CancellationToken cancellationToken)
        {
            var qcStatusId = request.QcStatusId;
            var statusCode = (await _queryRepository.GetQcStatusCodeByIdAsync(qcStatusId)
                ?? throw new ExceptionRules("Invalid QC Status.")).Trim().ToUpperInvariant();

            // Putaway-allowed statuses: Approved + Conditionally Approved.
            var isQcApproved = statusCode is "APR" or "CAP";

            var ctx = await _queryRepository.GetDispositionContextAsync(request.QcInspectionHdrId)
                ?? throw new ExceptionRules("Inspection not found.");

            // Evaluate Pass/Fail for the incoming readings (same logic as the parameter-save flow).
            var evalRows = (await _queryRepository.GetDetailEvaluationRowsAsync(request.QcInspectionHdrId))
                .ToDictionary(r => r.Id);

            var results = new List<(int DetailId, string? ActualValue, string? InspectionResult, string? Remarks)>();
            foreach (var p in request.Parameters)
            {
                evalRows.TryGetValue(p.DetailId, out var rule);
                var inspectionResult = _evaluator.Evaluate(
                    rule?.ValidationTypeCode, p.ActualValue,
                    rule?.MinValue, rule?.MaxValue, rule?.ExpectedValue, rule?.AllowedValues);

                results.Add((p.DetailId, p.ActualValue, inspectionResult, p.Remarks));
            }

            var dispositionByUserId = _ipAddressService.GetUserId();
            var dispositionByName = _ipAddressService.GetUserName();
            var qcApprovedIp = _ipAddressService.GetUserIPAddress();

            // Readings + disposition + GRN write-back, all in ONE transaction.
            await _commandRepository.SaveResultsAndDispositionAsync(
                request.QcInspectionHdrId, results,
                qcStatusId, request.AcceptedQuantity, request.RejectedQuantity, request.DispositionRemarks,
                dispositionByUserId, dispositionByName,
                qcApprovedIp, isQcApproved,
                ctx.GrnHeaderId, ctx.GrnDetailId);

            var grn = await _grnLookup.GetByGrnDetailIdAsync(ctx.GrnDetailId, cancellationToken);

            var movementEvent = new QcDispositionCompletedDomainEvent(
                QcInspectionHdrId: request.QcInspectionHdrId,
                QcInspectionNo: ctx.QcInspectionNo ?? string.Empty,
                GrnHeaderId: ctx.GrnHeaderId,
                GrnDetailId: ctx.GrnDetailId,
                ItemId: grn?.ItemId ?? 0,
                QcStatusId: qcStatusId,
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
